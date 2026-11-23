using BibliotecaApi.Domain.Entities;

namespace BibliotecaApi.Domain.Interfaces;

public interface ILibroRepository
{
    // GET /api/libros -> Listar todos los libros
    Task<IEnumerable<Libro>> ObtenerTodosAsync();

    // GET /api/libros/{id} -> Obtener un libro por su ID
    Task<Libro?> ObtenerPorIdAsync(int id);

    // POST /api/libros -> Crear un nuevo libro
    Task<Libro> CrearAsync(Libro libro);

    // PUT /api/libros/{id} -> Modificar un libro existente
    Task<bool> ActualizarAsync(Libro libro);

    // DELETE /api/libros/{id} -> Eliminar un libro por su ID
    Task<bool> EliminarAsync(int id);

    // Validación previa al DELETE -> Comprobar si tiene préstamos asociados
    Task<bool> TienePrestamosAsociadosAsync(int id);
}