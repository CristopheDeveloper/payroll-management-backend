using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class DepartamentoGuardarDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre debe tener como máximo 100 caracteres.")]
    public string Nombre { get; set; }

    [StringLength(200, ErrorMessage = "La descripción debe tener como máximo 200 caracteres.")]
    public string Descripcion { get; set; }
}
