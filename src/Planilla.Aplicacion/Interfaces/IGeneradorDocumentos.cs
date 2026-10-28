using Planilla.Aplicacion.DTOs;

namespace Planilla.Aplicacion.Interfaces;

public interface IGeneradorDocumentos
{
    byte[] PlanillaPdf(ReportePlanillaDto reporte);
    byte[] BoletasPdf(ReporteBoletasDto reporte);
    byte[] PlanillaExcel(ReportePlanillaDto reporte);
    byte[] TablaPdf(ReporteTablaDto reporte);
    byte[] TablaExcel(ReporteTablaDto reporte);
}
