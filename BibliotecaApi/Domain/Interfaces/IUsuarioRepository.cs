using BibliotecaApi.Domain.Entities;

namespace BibliotecaApi.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<IEnumerable<Usuario>> ObtenerTodosAsync();

    Task<Usuario?> ObtenerPorIdAsync(int id);

    Task<Usuario> CrearAsync(Usuario usuario);

    Task<bool> ExisteCorreoAsync(string correo);
    Task<bool> ExisteTipoUsuarioAsync(int idTipoUsuario);
}