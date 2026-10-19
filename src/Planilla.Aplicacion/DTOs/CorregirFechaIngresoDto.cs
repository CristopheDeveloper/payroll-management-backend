using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class CorregirFechaIngresoDto
{
    [Required(ErrorMessage = "La fecha de ingreso es obligatoria.")]
    public DateOnly? FechaIngreso { get; set; }
}
