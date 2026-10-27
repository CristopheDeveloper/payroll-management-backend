using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class RegistroAsistenciaDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El empleado es obligatorio.")]
    public int IdEmpleado { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [AllowedValues("PRESENTE", "FALTA", "PERMISO", "SUSPENSION_IGSS", ErrorMessage = "El estado debe ser PRESENTE, FALTA, PERMISO o SUSPENSION_IGSS.")]
    public string Estado { get; set; }

    [Range(typeof(decimal), "0", "999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "Las horas extra deben estar entre 0 y 999.99.")]
    public decimal HorasExtra { get; set; }

    [StringLength(200, ErrorMessage = "La observación debe tener como máximo 200 caracteres.")]
    public string Observacion { get; set; }
}
