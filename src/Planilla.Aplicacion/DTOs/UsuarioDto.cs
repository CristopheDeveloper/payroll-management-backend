namespace Planilla.Aplicacion.DTOs;

public class UsuarioDto
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; }
    public string NombreCompleto { get; set; }
    public int IdRol { get; set; }
    public string Rol { get; set; }
    public bool Activo { get; set; }
    public DateTime? UltimoAcceso { get; set; }
}
