using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class RangoAsistenciaDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El empleado es obligatorio.")]
    public int IdEmpleado { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [AllowedValues("PERMISO", "SUSPENSION_IGSS", ErrorMessage = "El estado debe ser PERMISO o SUSPENSION_IGSS.")]
    public string Estado { get; set; }

    [Required(ErrorMessage = "La fecha inicial es obligatoria.")]
    public DateOnly? Desde { get; set; }

    [Required(ErrorMessage = "La fecha final es obligatoria.")]
    public DateOnly? Hasta { get; set; }

    public bool SoloDiasHabiles { get; set; } = true;

    [StringLength(200, ErrorMessage = "La observación debe tener como máximo 200 caracteres.")]
    public string Observacion { get; set; }
}
