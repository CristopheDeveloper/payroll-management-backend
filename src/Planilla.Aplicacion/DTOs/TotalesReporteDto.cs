namespace Planilla.Aplicacion.DTOs;

public class TotalesReporteDto
{
    public int CantidadEmpleados { get; set; }
    public decimal SalarioDevengado { get; set; }
    public decimal MontoHorasExtra { get; set; }
    public decimal Bonificacion { get; set; }
    public decimal OtrosIngresos { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal IgssLaboral { get; set; }
    public decimal Isr { get; set; }
    public decimal OtrosDescuentos { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal LiquidoRecibir { get; set; }
}
