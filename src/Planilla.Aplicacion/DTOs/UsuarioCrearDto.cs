using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class UsuarioCrearDto
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(50, ErrorMessage = "El usuario debe tener como máximo 50 caracteres.")]
    [RegularExpression(@"^\s*[a-zA-Z0-9._-]+\s*$", ErrorMessage = "El usuario solo puede tener letras sin tilde ni ñ, números, punto, guion y guion bajo.")]
    public string NombreUsuario { get; set; }

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre completo debe tener como máximo 150 caracteres.")]
    public string NombreCompleto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El rol es obligatorio.")]
    public int IdRol { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
    public string Contrasena { get; set; }
}
