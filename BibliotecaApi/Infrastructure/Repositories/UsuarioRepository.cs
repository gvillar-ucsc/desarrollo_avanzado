using BibliotecaApi.Domain.Entities;
using BibliotecaApi.Domain.Interfaces;
using Npgsql;

namespace BibliotecaApi.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly string _connectionString;

    public UsuarioRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");
    }

    public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
    {
        var usuarios = new List<Usuario>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
            SELECT u.id_usuario, u.nombre, u.correo, u.id_tipo_usuario, t.nombre
            FROM usuario u
            INNER JOIN tipo_usuario t ON u.id_tipo_usuario = t.id_tipo_usuario
            ORDER BY u.id_usuario;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            usuarios.Add(new Usuario
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Correo = reader.GetString(2),
                IdTipoUsuario = reader.GetInt32(3),
                TipoUsuario = reader.GetString(4)
            });
        }

        return usuarios;
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
            SELECT u.id_usuario, u.nombre, u.correo, u.id_tipo_usuario, t.nombre
            FROM usuario u
            INNER JOIN tipo_usuario t ON u.id_tipo_usuario = t.id_tipo_usuario
            WHERE u.id_usuario = @id;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Usuario
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Correo = reader.GetString(2),
                IdTipoUsuario = reader.GetInt32(3),
                TipoUsuario = reader.GetString(4)
            };
        }

        return null;
    }

    public async Task<Usuario> CrearAsync(Usuario usuario)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
            INSERT INTO usuario (nombre, correo, id_tipo_usuario)
            VALUES (@nombre, @correo, @idTipoUsuario)
            RETURNING id_usuario;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nombre", usuario.Nombre);
        cmd.Parameters.AddWithValue("@correo", usuario.Correo);
        cmd.Parameters.AddWithValue("@idTipoUsuario", usuario.IdTipoUsuario);

        var idGenerado = await cmd.ExecuteScalarAsync();
        usuario.Id = Convert.ToInt32(idGenerado);

        return usuario;
    }

    public async Task<bool> ExisteCorreoAsync(string correo)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = "SELECT COUNT(1) FROM usuario WHERE LOWER(correo) = LOWER(@correo);";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@correo", correo);

        var cantidad = Convert.ToInt64(await cmd.ExecuteScalarAsync());
        return cantidad > 0;
    }

    public async Task<bool> ExisteTipoUsuarioAsync(int idTipoUsuario)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = "SELECT COUNT(1) FROM tipo_usuario WHERE id_tipo_usuario = @id;";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", idTipoUsuario);

        var cantidad = Convert.ToInt64(await cmd.ExecuteScalarAsync());
        return cantidad > 0;
    }
}