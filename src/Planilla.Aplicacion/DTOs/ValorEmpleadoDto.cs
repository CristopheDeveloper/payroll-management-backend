using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class ValorEmpleadoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El empleado es obligatorio.")]
    public int IdEmpleado { get; set; }

    [Range(typeof(decimal), "0", "999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El valor debe estar entre 0 y 999,999.99.")]
    public decimal Valor { get; set; }

    [StringLength(200, ErrorMessage = "La descripción debe tener como máximo 200 caracteres.")]
    public string Descripcion { get; set; }
}
