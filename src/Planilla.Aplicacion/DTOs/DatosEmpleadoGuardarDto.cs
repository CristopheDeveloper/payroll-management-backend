using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class DatosEmpleadoGuardarDto
{
    [Range(0, 366, ErrorMessage = "Los días a pagar deben estar entre 0 y 366.")]
    public int DiasPagar { get; set; }

    [Range(typeof(decimal), "0", "999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "Las horas extra deben estar entre 0 y 999.99.")]
    public decimal HorasExtra { get; set; }

    [Range(typeof(decimal), "0", "999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El ISR debe estar entre 0 y 999,999.99.")]
    public decimal Isr { get; set; }

    [Required(ErrorMessage = "La lista de valores es obligatoria (puede ir vacía).")]
    public List<ValorGuardarDto> Valores { get; set; }
}
