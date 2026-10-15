namespace Planilla.Aplicacion.DTOs;

public class NominaDetalleDto
{
    public int IdNominaDetalle { get; set; }
    public int IdEmpleado { get; set; }
    public string CodigoEmpleado { get; set; }
    public string NombreCompleto { get; set; }
    public int IdPuesto { get; set; }
    public string NombrePuesto { get; set; }
    public int IdDepartamento { get; set; }
    public string NombreDepartamento { get; set; }
    public decimal Salario { get; set; }
    public string TipoPago { get; set; }
    public decimal RetencionIsr { get; set; }
    public decimal Isr { get; set; }
    public int DiasPeriodo { get; set; }
    public int DiasFalta { get; set; }
    public int DiasSuspension { get; set; }
    public int DiasPagar { get; set; }
    public decimal HorasExtra { get; set; }
    public decimal SalarioDevengado { get; set; }
    public decimal MontoHorasExtra { get; set; }
    public decimal Bonificacion { get; set; }
    public decimal OtrosIngresosAfectos { get; set; }
    public decimal OtrosIngresosNoAfectos { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal IgssLaboral { get; set; }
    public decimal OtrosDescuentos { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal LiquidoRecibir { get; set; }
    public decimal IgssPatronal { get; set; }
    public decimal Irtra { get; set; }
    public decimal Intecap { get; set; }
    public List<AdvertenciaDto> Advertencias { get; set; }
}
