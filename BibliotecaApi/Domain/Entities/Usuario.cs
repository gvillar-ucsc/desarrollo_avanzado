namespace BibliotecaApi.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public int IdTipoUsuario { get; set; }
    public string? TipoUsuario { get; set; }
}