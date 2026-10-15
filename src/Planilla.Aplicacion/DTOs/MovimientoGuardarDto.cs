using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class MovimientoGuardarDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El empleado es obligatorio.")]
    public int IdEmpleado { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El rubro es obligatorio.")]
    public int IdRubro { get; set; }

    [Range(typeof(decimal), "0.01", "99999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El monto debe estar entre 0.01 y 99,999,999.99.")]
    public decimal Monto { get; set; }

    [StringLength(200, ErrorMessage = "La descripción debe tener como máximo 200 caracteres.")]
    public string Descripcion { get; set; }
}
