using Planilla.Aplicacion.DTOs;

namespace Planilla.Aplicacion.Interfaces;

public interface IReporteCatalogoServicio
{
    Task<ReporteTablaDto> PlanillaGeneral(int idNomina);
    Task<ReporteTablaDto> Boletas(int idNomina);
    Task<ReporteTablaDto> FormaPago(int idNomina, string tipoPago);
    Task<ReporteTablaDto> Rubros(int idNomina, int? idRubro);
    Task<ReporteTablaDto> Igss(int idNomina);
    Task<ReporteTablaDto> Poliza(int idNomina);
    Task<ReporteTablaDto> Empleados(string estado, int? idDepartamento, int? idPuesto);
    Task<ReporteTablaDto> AltasBajas(DateOnly desde, DateOnly hasta);
    Task<ReporteTablaDto> Historial(int idEmpleado, DateOnly? desde, DateOnly? hasta);
    Task<ReporteTablaDto> Asistencia(int anio, int mes, int? idDepartamento);
}
