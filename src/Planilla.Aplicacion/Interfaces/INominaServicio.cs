using Planilla.Aplicacion.DTOs;

namespace Planilla.Aplicacion.Interfaces;

public interface INominaServicio
{
    Task<List<NominaDto>> ObtenerTodas(int? anio);
    Task<NominaDto> ObtenerPorId(int idNomina);
    Task<NominaDto> Crear(NominaCrearDto dto);
    Task Eliminar(int idNomina);
    Task<List<EmpleadoMesDto>> ObtenerEmpleados(int idNomina);
    Task<List<NominaDetalleDto>> ObtenerDetalle(int idNomina);
    Task<ComparacionDto> ObtenerComparacion(int idNomina);
    Task<NominaDto> Calcular(int idNomina, CalcularDto dto);
    Task<NominaDto> Cerrar(int idNomina, CerrarDto dto);
    Task<NominaDto> Reabrir(int idNomina, ReabrirDto dto);
}
