namespace Planilla.Dominio.Calculo;

public class DatosEmpleadoMes
{
    public decimal Salario { get; set; }
    public decimal Isr { get; set; }
    public int DiasPagar { get; set; }
    public decimal HorasExtra { get; set; }
    public List<ValorManual> Valores { get; set; } = [];
}
