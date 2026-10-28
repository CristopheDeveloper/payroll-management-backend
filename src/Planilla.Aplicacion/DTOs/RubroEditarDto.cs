using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class RubroEditarDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre debe tener como máximo 100 caracteres.")]
    public string Nombre { get; set; }

    public bool AfectoIgss { get; set; }

    [Range(1, 999, ErrorMessage = "El orden debe estar entre 1 y 999.")]
    public int Orden { get; set; }

    [StringLength(200, ErrorMessage = "La descripción debe tener como máximo 200 caracteres.")]
    public string Descripcion { get; set; }

    [RegularExpression("^[0-9]{4}$", ErrorMessage = "La cuenta contable debe tener 4 dígitos.")]
    public string CuentaContable { get; set; }
}
