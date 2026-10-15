using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class UsuarioEditarDto
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre completo debe tener como máximo 150 caracteres.")]
    public string NombreCompleto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El rol es obligatorio.")]
    public int IdRol { get; set; }
}
