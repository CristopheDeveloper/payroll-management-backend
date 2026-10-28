using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class VariableGuardarDto
{
    [Required(ErrorMessage = "La variable es obligatoria.")]
    [AllowedValues("DIAS_PAGAR", "HORAS_EXTRA", "ISR", "RUBRO", ErrorMessage = "La variable debe ser DIAS_PAGAR, HORAS_EXTRA, ISR o RUBRO.")]
    public string Variable { get; set; }

    public int? IdRubro { get; set; }

    [Required(ErrorMessage = "La lista de valores es obligatoria.")]
    [MinLength(1, ErrorMessage = "La lista de valores no puede estar vacía.")]
    public List<ValorEmpleadoDto> Valores { get; set; }
}
