using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class CerrarDto
{
    [Required(ErrorMessage = "La versión es obligatoria; recargue la planilla antes de cerrar.")]
    public string Version { get; set; }
}
