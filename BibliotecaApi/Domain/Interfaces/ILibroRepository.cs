using BibliotecaApi.Domain.Entities;

namespace BibliotecaApi.Domain.Interfaces;

public interface ILibroRepository
{
    Task<IEnumerable<Libro>> ObtenerTodosAsync();

    Task<Libro?> ObtenerPorIdAsync(int id);

    Task<Libro> CrearAsync(Libro libro);

    Task<bool> ActualizarAsync(Libro libro);

    Task<bool> EliminarAsync(int id);

    Task<bool> TienePrestamosAsociadosAsync(int id);
}