using BibliotecaApi.Domain.Entities;
using BibliotecaApi.Domain.Interfaces;

namespace BibliotecaApi.Application.Services;

public class UsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuarioService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<IEnumerable<Usuario>> ListarAsync()
    {
        return await _usuarioRepository.ObtenerTodosAsync();
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        return await _usuarioRepository.ObtenerPorIdAsync(id);
    }

    public async Task<(int StatusCode, object? Data, string? Error)> CrearAsync(Usuario usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario.Nombre))
            return (400, null, "El nombre del usuario no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(usuario.Correo))
            return (400, null, "El correo electrónico no puede estar vacío.");

        usuario.Nombre = usuario.Nombre.Trim();
        usuario.Correo = usuario.Correo.Trim().ToLowerInvariant();

        if (!await _usuarioRepository.ExisteTipoUsuarioAsync(usuario.IdTipoUsuario))
            return (400, null, "El tipo de usuario especificado no existe.");

        if (await _usuarioRepository.ExisteCorreoAsync(usuario.Correo))
            return (409, null, "Ya existe un usuario registrado con ese correo electrónico.");

        var usuarioCreado = await _usuarioRepository.CrearAsync(usuario);
        var usuarioCompleto = await _usuarioRepository.ObtenerPorIdAsync(usuarioCreado.Id);

        return (201, usuarioCompleto ?? usuarioCreado, null);
    }
}