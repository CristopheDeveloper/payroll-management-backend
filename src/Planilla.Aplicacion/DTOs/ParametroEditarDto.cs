using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class ParametroEditarDto
{
    [Required(ErrorMessage = "El valor es obligatorio.")]
    [StringLength(100, ErrorMessage = "El valor debe tener como máximo 100 caracteres.")]
    public string Valor { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(200, ErrorMessage = "La descripción debe tener como máximo 200 caracteres.")]
    public string Descripcion { get; set; }
}
