using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class ReabrirDto
{
    [Required(ErrorMessage = "El motivo es obligatorio.")]
    [StringLength(200, ErrorMessage = "El motivo debe tener como máximo 200 caracteres.")]
    public string Motivo { get; set; }
}
