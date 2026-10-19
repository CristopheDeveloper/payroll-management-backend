namespace Planilla.Aplicacion.DTOs;

public class ReportePlanillaDto
{
    public EncabezadoReporteDto Encabezado { get; set; }
    public List<DepartamentoReporteDto> Departamentos { get; set; }
    public TotalesReporteDto Total { get; set; }
    public CuotaPatronalDto CuotaPatronal { get; set; }
}
