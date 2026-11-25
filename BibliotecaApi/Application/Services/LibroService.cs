using BibliotecaApi.Domain.Entities;
using BibliotecaApi.Domain.Interfaces;

namespace BibliotecaApi.Application.Services;

public class LibroService
{
    private readonly ILibroRepository _libroRepository;

    public LibroService(ILibroRepository libroRepository)
    {
        _libroRepository = libroRepository;
    }

    public async Task<IEnumerable<Libro>> ListarAsync()
    {
        return await _libroRepository.ObtenerTodosAsync();
    }

    public async Task<Libro?> ObtenerPorIdAsync(int id)
    {
        return await _libroRepository.ObtenerPorIdAsync(id);
    }

    public async Task<(int StatusCode, object? Data, string? Error)> CrearAsync(Libro libro)
    {
        if (string.IsNullOrWhiteSpace(libro.Titulo))
            return (400, null, "El título del libro no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(libro.Autor))
            return (400, null, "El autor del libro no puede estar vacío.");

        var libroCreado = await _libroRepository.CrearAsync(libro);
        return (201, libroCreado, null);
    }

    public async Task<(int StatusCode, string? Error)> ActualizarAsync(int id, Libro libro)
    {
        if (string.IsNullOrWhiteSpace(libro.Titulo))
            return (400, "El título del libro no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(libro.Autor))
            return (400, "El autor del libro no puede estar vacío.");

        var existente = await _libroRepository.ObtenerPorIdAsync(id);
        if (existente is null)
            return (404, "El libro especificado no existe.");

        libro.Id = id;
        await _libroRepository.ActualizarAsync(libro);
        return (204, null);
    }

    public async Task<(int StatusCode, string? Error)> EliminarAsync(int id)
    {
        var existente = await _libroRepository.ObtenerPorIdAsync(id);
        if (existente is null)
            return (404, "El libro especificado no existe.");

        if (await _libroRepository.TienePrestamosAsociadosAsync(id))
            return (409, "No se puede eliminar el libro porque tiene préstamos asociados.");

        await _libroRepository.EliminarAsync(id);
        return (204, null);
    }
}