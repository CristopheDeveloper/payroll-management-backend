namespace Planilla.Aplicacion.DTOs;

public class PagoEmpleadoDto
{
    public int IdNomina { get; set; }
    public string Codigo { get; set; }
    public string Descripcion { get; set; }
    public string NombreTipoPlanilla { get; set; }
    public int Anio { get; set; }
    public int Mes { get; set; }
    public DateOnly FechaPago { get; set; }
    public int DiasPagar { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal Liquido { get; set; }
    public List<NominaRubroDto> Rubros { get; set; }
}
