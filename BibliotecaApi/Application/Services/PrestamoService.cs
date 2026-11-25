using BibliotecaApi.Domain.Entities;
using BibliotecaApi.Domain.Interfaces;
using Npgsql;

namespace BibliotecaApi.Application.Services;

public class PrestamoService
{
    private readonly IPrestamoRepository _prestamoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILibroRepository _libroRepository;

    public PrestamoService(
        IPrestamoRepository prestamoRepository,
        IUsuarioRepository usuarioRepository,
        ILibroRepository libroRepository)
    {
        _prestamoRepository = prestamoRepository;
        _usuarioRepository = usuarioRepository;
        _libroRepository = libroRepository;
    }

    public async Task<IEnumerable<Prestamo>> ListarAsync()
    {
        return await _prestamoRepository.ObtenerTodosAsync();
    }

    public async Task<(int StatusCode, object? Data, string? Error)> RegistrarPrestamoAsync(int idUsuario, int idLibro)
    {
        if (idUsuario <= 0 || idLibro <= 0)
            return (400, null, "Debe especificar identificadores válidos de usuario y libro.");

        var usuario = await _usuarioRepository.ObtenerPorIdAsync(idUsuario);
        if (usuario is null)
            return (404, null, "El usuario especificado no existe.");

        var libro = await _libroRepository.ObtenerPorIdAsync(idLibro);
        if (libro is null)
            return (404, null, "El libro especificado no existe.");

        if (!libro.Disponible)
            return (409, null, "El libro no se encuentra disponible para préstamo.");

        try
        {
            var prestamoCreado = await _prestamoRepository.RegistrarPrestamoAsync(idUsuario, idLibro);
            var prestamoCompleto = await _prestamoRepository.ObtenerPorIdAsync(prestamoCreado.Id);
            return (201, prestamoCompleto ?? prestamoCreado, null);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            // Respaldo ante el índice único uq_prestamo_libro_activo de PostgreSQL
            return (409, null, "El libro ya tiene un préstamo activo.");
        }
    }

    public async Task<(int StatusCode, object? Data, string? Error)> RegistrarDevolucionAsync(int idPrestamo)
    {
        var prestamo = await _prestamoRepository.ObtenerPorIdAsync(idPrestamo);
        if (prestamo is null)
            return (404, null, "El préstamo especificado no existe.");

        if (prestamo.FechaDevolucion is not null)
            return (409, null, "Este préstamo ya fue devuelto anteriormente.");

        await _prestamoRepository.RegistrarDevolucionAsync(idPrestamo, prestamo.IdLibro);
        var prestamoActualizado = await _prestamoRepository.ObtenerPorIdAsync(idPrestamo);

        return (200, prestamoActualizado, null);
    }
}