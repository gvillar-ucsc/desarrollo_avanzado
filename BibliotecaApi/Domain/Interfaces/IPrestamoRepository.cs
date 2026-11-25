using BibliotecaApi.Domain.Entities;

namespace BibliotecaApi.Domain.Interfaces;

public interface IPrestamoRepository
{
    Task<IEnumerable<Prestamo>> ObtenerTodosAsync();

    Task<Prestamo?> ObtenerPorIdAsync(int id);

    Task<Prestamo> RegistrarPrestamoAsync(int idUsuario, int idLibro);

    Task<bool> RegistrarDevolucionAsync(int idPrestamo, int idLibro);
}