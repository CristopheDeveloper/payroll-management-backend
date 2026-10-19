namespace Planilla.Aplicacion.DTOs;

public class EmpleadoDto
{
    public int IdEmpleado { get; set; }
    public string CodigoEmpleado { get; set; }
    public string NombreCompleto { get; set; }
    public string Dpi { get; set; }
    public string Puesto { get; set; }
    public string Departamento { get; set; }
    public string TipoPago { get; set; }
    public decimal Salario { get; set; }
    public bool Activo { get; set; }
    public DateOnly? FechaIngresoActual { get; set; }
    public DateOnly? FechaBajaUltima { get; set; }
}
