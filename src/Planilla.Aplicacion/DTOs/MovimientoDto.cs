namespace Planilla.Aplicacion.DTOs;

public class MovimientoDto
{
    public int IdMovimiento { get; set; }
    public int IdEmpleado { get; set; }
    public string CodigoEmpleado { get; set; }
    public string NombreCompleto { get; set; }
    public int IdRubro { get; set; }
    public string Rubro { get; set; }
    public string Tipo { get; set; }
    public bool AfectoIgss { get; set; }
    public string Descripcion { get; set; }
    public decimal Monto { get; set; }
}
