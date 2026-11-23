using BibliotecaApi.Domain.Entities;
using BibliotecaApi.Domain.Interfaces;

namespace BibliotecaApi.Application.Services;

public class LibroService
{
    private readonly ILibroRepository _libroRepository;

    // Inyección de dependencias: recibe la interfaz del repositorio
    public LibroService(ILibroRepository libroRepository)
    {
        _libroRepository = libroRepository;
    }

    // 1. Listar todos los libros
    public async Task<IEnumerable<Libro>> ListarAsync()
    {
        return await _libroRepository.ObtenerTodosAsync();
    }

    // 2. Obtener un libro por ID
    public async Task<Libro?> ObtenerPorIdAsync(int id)
    {
        return await _libroRepository.ObtenerPorIdAsync(id);
    }

    // 3. Crear libro (con validación de campos vacíos)
    public async Task<(int StatusCode, object? Data, string? Error)> CrearAsync(Libro libro)
    {
        if (string.IsNullOrWhiteSpace(libro.Titulo))
            return (400, null, "El título del libro no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(libro.Autor))
            return (400, null, "El autor del libro no puede estar vacío.");

        var libroCreado = await _libroRepository.CrearAsync(libro);
        return (201, libroCreado, null);
    }

    // 4. Modificar libro (valida campos vacíos y que el ID exista)
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

    // 5. Eliminar libro (valida que exista y que NO tenga préstamos asociados)
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