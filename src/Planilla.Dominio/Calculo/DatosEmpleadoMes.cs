namespace Planilla.Dominio.Calculo;

public class DatosEmpleadoMes
{
    public decimal Salario { get; set; }
    public decimal RetencionIsr { get; set; }
    public int IdPuesto { get; set; }
    public int IdDepartamento { get; set; }
    public string TipoPago { get; set; }
    public int DiasPeriodo { get; set; }
    public int DiasFalta { get; set; }
    public int DiasSuspension { get; set; }
    public int DiasPagar { get; set; }
    public decimal HorasExtra { get; set; }
    public decimal OtrosIngresosAfectos { get; set; }
    public decimal OtrosIngresosNoAfectos { get; set; }
    public decimal OtrosDescuentos { get; set; }
}
