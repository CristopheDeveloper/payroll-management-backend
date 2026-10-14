using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class LoginDto
{
    [Required]
    [StringLength(50)]
    public string NombreUsuario { get; set; }

    [Required]
    public string Contrasena { get; set; }
}
