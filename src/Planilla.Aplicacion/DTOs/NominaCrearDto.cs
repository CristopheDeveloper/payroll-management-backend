using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class NominaCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El tipo de planilla es obligatorio.")]
    public int IdTipoPlanilla { get; set; }

    [Range(2000, 2100, ErrorMessage = "El año debe estar entre 2000 y 2100.")]
    public int Anio { get; set; }

    [Range(1, 12, ErrorMessage = "El mes debe estar entre 1 y 12.")]
    public int Mes { get; set; }

    [StringLength(20, ErrorMessage = "El código debe tener como máximo 20 caracteres.")]
    [RegularExpression("^[A-Za-z0-9_-]+$", ErrorMessage = "El código solo puede tener letras sin tilde, números, guion y guion bajo.")]
    public string Codigo { get; set; }

    [StringLength(200, ErrorMessage = "La descripción debe tener como máximo 200 caracteres.")]
    public string Descripcion { get; set; }

    public DateOnly? FechaPago { get; set; }

    [StringLength(500, ErrorMessage = "El comentario debe tener como máximo 500 caracteres.")]
    public string Comentario { get; set; }
}
