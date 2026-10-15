using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class ReingresoDto
{
    [Required(ErrorMessage = "La fecha de ingreso es obligatoria.")]
    public DateOnly? FechaIngreso { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El puesto es obligatorio.")]
    public int IdPuesto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El departamento es obligatorio.")]
    public int IdDepartamento { get; set; }

    [Range(typeof(decimal), "0.01", "99999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El salario debe estar entre 0.01 y 99,999,999.99.")]
    public decimal Salario { get; set; }
}
