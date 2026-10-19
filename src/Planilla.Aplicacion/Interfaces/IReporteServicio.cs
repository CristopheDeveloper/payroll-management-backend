using Planilla.Aplicacion.DTOs;

namespace Planilla.Aplicacion.Interfaces;

public interface IReporteServicio
{
    Task<ReportePlanillaDto> ObtenerPlanilla(int idNomina);
    Task<ReporteBoletasDto> ObtenerBoletas(int idNomina, int? idEmpleado);
}
