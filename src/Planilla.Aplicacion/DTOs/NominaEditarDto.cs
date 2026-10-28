using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class NominaEditarDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(20, ErrorMessage = "El código debe tener como máximo 20 caracteres.")]
    [RegularExpression("^[A-Za-z0-9_-]+$", ErrorMessage = "El código solo puede tener letras sin tilde, números, guion y guion bajo.")]
    public string Codigo { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(200, ErrorMessage = "La descripción debe tener como máximo 200 caracteres.")]
    public string Descripcion { get; set; }

    [Required(ErrorMessage = "La fecha de pago es obligatoria.")]
    public DateOnly? FechaPago { get; set; }

    [StringLength(500, ErrorMessage = "El comentario debe tener como máximo 500 caracteres.")]
    public string Comentario { get; set; }
}
