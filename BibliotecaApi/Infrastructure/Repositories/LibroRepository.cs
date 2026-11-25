using BibliotecaApi.Domain.Entities;
using BibliotecaApi.Domain.Interfaces;
using Npgsql;

namespace BibliotecaApi.Infrastructure.Repositories;

public class LibroRepository : ILibroRepository
{
    private readonly string _connectionString;

    public LibroRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");
    }

    public async Task<IEnumerable<Libro>> ObtenerTodosAsync()
    {
        var libros = new List<Libro>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = "SELECT id_libro, titulo, autor, disponible FROM libro ORDER BY id_libro;";
        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            libros.Add(new Libro
            {
                Id = reader.GetInt32(0),
                Titulo = reader.GetString(1),
                Autor = reader.GetString(2),
                Disponible = reader.GetBoolean(3)
            });
        }

        return libros;
    }

    public async Task<Libro?> ObtenerPorIdAsync(int id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = "SELECT id_libro, titulo, autor, disponible FROM libro WHERE id_libro = @id;";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Libro
            {
                Id = reader.GetInt32(0),
                Titulo = reader.GetString(1),
                Autor = reader.GetString(2),
                Disponible = reader.GetBoolean(3)
            };
        }

        return null;
    }

    public async Task<Libro> CrearAsync(Libro libro)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
            INSERT INTO libro (titulo, autor, disponible)
            VALUES (@titulo, @autor, @disponible)
            RETURNING id_libro;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@titulo", libro.Titulo);
        cmd.Parameters.AddWithValue("@autor", libro.Autor);
        cmd.Parameters.AddWithValue("@disponible", libro.Disponible);

        var idGenerado = await cmd.ExecuteScalarAsync();
        libro.Id = Convert.ToInt32(idGenerado);

        return libro;
    }

    public async Task<bool> ActualizarAsync(Libro libro)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
            UPDATE libro
            SET titulo = @titulo, autor = @autor, disponible = @disponible
            WHERE id_libro = @id;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", libro.Id);
        cmd.Parameters.AddWithValue("@titulo", libro.Titulo);
        cmd.Parameters.AddWithValue("@autor", libro.Autor);
        cmd.Parameters.AddWithValue("@disponible", libro.Disponible);

        int filasAfectadas = await cmd.ExecuteNonQueryAsync();
        return filasAfectadas > 0;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = "DELETE FROM libro WHERE id_libro = @id;";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        int filasAfectadas = await cmd.ExecuteNonQueryAsync();
        return filasAfectadas > 0;
    }

    public async Task<bool> TienePrestamosAsociadosAsync(int id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = "SELECT COUNT(1) FROM prestamo WHERE id_libro = @id;";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        var cantidad = Convert.ToInt64(await cmd.ExecuteScalarAsync());
        return cantidad > 0;
    }
}