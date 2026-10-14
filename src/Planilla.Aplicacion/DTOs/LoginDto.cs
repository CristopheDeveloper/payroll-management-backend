using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(50, ErrorMessage = "El usuario debe tener como máximo 50 caracteres.")]
    public string NombreUsuario { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, ErrorMessage = "La contraseña debe tener como máximo 100 caracteres.")]
    public string Contrasena { get; set; }
}
