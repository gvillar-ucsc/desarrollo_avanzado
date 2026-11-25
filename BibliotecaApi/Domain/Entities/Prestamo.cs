namespace BibliotecaApi.Domain.Entities;

public class Prestamo
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string? NombreUsuario { get; set; }
    public int IdLibro { get; set; }
    public string? TituloLibro { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime? FechaDevolucion { get; set; }
}