namespace Planilla.Aplicacion.DTOs;

public class BoletaDto
{
    public int IdEmpleado { get; set; }
    public string CodigoEmpleado { get; set; }
    public string NombreCompleto { get; set; }
    public string NombrePuesto { get; set; }
    public string NombreDepartamento { get; set; }
    public string TipoPago { get; set; }
    public int DiasPagar { get; set; }
    public List<LineaBoletaDto> Ingresos { get; set; }
    public List<LineaBoletaDto> Descuentos { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal LiquidoRecibir { get; set; }
}
