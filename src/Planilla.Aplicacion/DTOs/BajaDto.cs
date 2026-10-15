using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class BajaDto
{
    [Required(ErrorMessage = "La fecha de baja es obligatoria.")]
    public DateOnly? FechaBaja { get; set; }

    [Required(ErrorMessage = "El tipo de baja es obligatorio.")]
    [AllowedValues("RENUNCIA", "DESPIDO", "FIN_CONTRATO", "OTRO", ErrorMessage = "El tipo de baja debe ser RENUNCIA, DESPIDO, FIN_CONTRATO u OTRO.")]
    public string TipoBaja { get; set; }

    [StringLength(200, ErrorMessage = "El motivo debe tener como máximo 200 caracteres.")]
    public string MotivoBaja { get; set; }
}
