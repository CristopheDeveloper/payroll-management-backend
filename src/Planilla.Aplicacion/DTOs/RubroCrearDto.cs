using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class RubroCrearDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre debe tener como máximo 100 caracteres.")]
    public string Nombre { get; set; }

    [Required(ErrorMessage = "El tipo es obligatorio.")]
    [AllowedValues("INGRESO", "DESCUENTO", ErrorMessage = "El tipo debe ser INGRESO o DESCUENTO.")]
    public string Tipo { get; set; }

    public bool AfectoIgss { get; set; }

    [StringLength(200, ErrorMessage = "La descripción debe tener como máximo 200 caracteres.")]
    public string Descripcion { get; set; }

    [StringLength(20, ErrorMessage = "La cuenta contable debe tener como máximo 20 caracteres.")]
    public string CuentaContable { get; set; }
}
