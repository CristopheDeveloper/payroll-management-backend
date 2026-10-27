using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class ImportarVariablesDto
{
    [Required(ErrorMessage = "La lista de variables es obligatoria.")]
    [MinLength(1, ErrorMessage = "La lista de variables no puede estar vacía.")]
    public List<VariableGuardarDto> Variables { get; set; }
}
