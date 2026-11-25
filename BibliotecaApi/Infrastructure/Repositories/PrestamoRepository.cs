using BibliotecaApi.Domain.Entities;
using BibliotecaApi.Domain.Interfaces;
using Npgsql;

namespace BibliotecaApi.Infrastructure.Repositories;

public class PrestamoRepository : IPrestamoRepository
{
    private readonly string _connectionString;

    public PrestamoRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");
    }

    public async Task<IEnumerable<Prestamo>> ObtenerTodosAsync()
    {
        var prestamos = new List<Prestamo>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
            SELECT p.id_prestamo, p.id_usuario, u.nombre, p.id_libro, l.titulo, p.fecha_prestamo, p.fecha_devolucion
            FROM prestamo p
            INNER JOIN usuario u ON p.id_usuario = u.id_usuario
            INNER JOIN libro l ON p.id_libro = l.id_libro
            ORDER BY p.id_prestamo DESC;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            prestamos.Add(new Prestamo
            {
                Id = reader.GetInt32(0),
                IdUsuario = reader.GetInt32(1),
                NombreUsuario = reader.GetString(2),
                IdLibro = reader.GetInt32(3),
                TituloLibro = reader.GetString(4),
                FechaPrestamo = reader.GetDateTime(5),
                FechaDevolucion = reader.IsDBNull(6) ? null : reader.GetDateTime(6)
            });
        }

        return prestamos;
    }

    public async Task<Prestamo?> ObtenerPorIdAsync(int id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
            SELECT p.id_prestamo, p.id_usuario, u.nombre, p.id_libro, l.titulo, p.fecha_prestamo, p.fecha_devolucion
            FROM prestamo p
            INNER JOIN usuario u ON p.id_usuario = u.id_usuario
            INNER JOIN libro l ON p.id_libro = l.id_libro
            WHERE p.id_prestamo = @id;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Prestamo
            {
                Id = reader.GetInt32(0),
                IdUsuario = reader.GetInt32(1),
                NombreUsuario = reader.GetString(2),
                IdLibro = reader.GetInt32(3),
                TituloLibro = reader.GetString(4),
                FechaPrestamo = reader.GetDateTime(5),
                FechaDevolucion = reader.IsDBNull(6) ? null : reader.GetDateTime(6)
            };
        }

        return null;
    }

    public async Task<Prestamo> RegistrarPrestamoAsync(int idUsuario, int idLibro)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // Inicio de la transacción (Desafío complementario)
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            const string sqlInsert = @"
                INSERT INTO prestamo (id_usuario, id_libro)
                VALUES (@idUsuario, @idLibro)
                RETURNING id_prestamo, fecha_prestamo;";

            int idGenerado;
            DateTime fechaPrestamo;

            await using (var cmdInsert = new NpgsqlCommand(sqlInsert, conn, tx))
            {
                cmdInsert.Parameters.AddWithValue("@idUsuario", idUsuario);
                cmdInsert.Parameters.AddWithValue("@idLibro", idLibro);

                await using var reader = await cmdInsert.ExecuteReaderAsync();
                await reader.ReadAsync();
                idGenerado = reader.GetInt32(0);
                fechaPrestamo = reader.GetDateTime(1);
            }

            const string sqlUpdateLibro = @"
                UPDATE libro
                SET disponible = FALSE
                WHERE id_libro = @idLibro;";

            await using (var cmdUpdate = new NpgsqlCommand(sqlUpdateLibro, conn, tx))
            {
                cmdUpdate.Parameters.AddWithValue("@idLibro", idLibro);
                await cmdUpdate.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();

            return new Prestamo
            {
                Id = idGenerado,
                IdUsuario = idUsuario,
                IdLibro = idLibro,
                FechaPrestamo = fechaPrestamo,
                FechaDevolucion = null
            };
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> RegistrarDevolucionAsync(int idPrestamo, int idLibro)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // Inicio de la transacción (Desafío complementario)
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            const string sqlUpdatePrestamo = @"
                UPDATE prestamo
                SET fecha_devolucion = CURRENT_TIMESTAMP
                WHERE id_prestamo = @idPrestamo AND fecha_devolucion IS NULL;";

            int filasAfectadas;
            await using (var cmdPrestamo = new NpgsqlCommand(sqlUpdatePrestamo, conn, tx))
            {
                cmdPrestamo.Parameters.AddWithValue("@idPrestamo", idPrestamo);
                filasAfectadas = await cmdPrestamo.ExecuteNonQueryAsync();
            }

            if (filasAfectadas == 0)
            {
                await tx.RollbackAsync();
                return false;
            }

            const string sqlUpdateLibro = @"
                UPDATE libro
                SET disponible = TRUE
                WHERE id_libro = @idLibro;";

            await using (var cmdLibro = new NpgsqlCommand(sqlUpdateLibro, conn, tx))
            {
                cmdLibro.Parameters.AddWithValue("@idLibro", idLibro);
                await cmdLibro.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return true;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}