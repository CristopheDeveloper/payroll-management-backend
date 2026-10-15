using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class RestablecerContrasenaDto
{
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
    public string Contrasena { get; set; }
}
