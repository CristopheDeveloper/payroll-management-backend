using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class DecisionDto
{
    public int? IdEmpleado { get; set; }

    [Required(ErrorMessage = "El campo de la decisión es obligatorio.")]
    public string Campo { get; set; }

    [Required(ErrorMessage = "El valor actual de la decisión es obligatorio.")]
    public string ValorActual { get; set; }

    [Required(ErrorMessage = "La decisión es obligatoria.")]
    [AllowedValues("ACTUAL", "GUARDADO", ErrorMessage = "La decisión debe ser ACTUAL o GUARDADO.")]
    public string Usar { get; set; }
}
