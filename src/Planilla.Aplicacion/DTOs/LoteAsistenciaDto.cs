using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class LoteAsistenciaDto
{
    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public DateOnly? Fecha { get; set; }

    [Required(ErrorMessage = "La versión del día es obligatoria; recargue el día antes de guardar.")]
    public string Version { get; set; }

    [Required(ErrorMessage = "La lista de asistencias es obligatoria.")]
    [MinLength(1, ErrorMessage = "La lista de asistencias no puede estar vacía.")]
    public List<RegistroAsistenciaDto> Registros { get; set; }
}
