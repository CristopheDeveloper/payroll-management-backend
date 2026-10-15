using Planilla.Aplicacion.DTOs;

namespace Planilla.Aplicacion.Interfaces;

public interface IAsistenciaServicio
{
    Task<DiaAsistenciaDto> ObtenerDia(DateOnly? fecha);
    Task<DiaAsistenciaDto> GuardarDia(LoteAsistenciaDto dto);
    Task<List<ResumenAsistenciaDto>> ObtenerResumen(int? anio, int? mes);
    Task<List<AsistenciaDto>> ObtenerPorEmpleado(int idEmpleado, int? anio, int? mes);
    Task<List<AsistenciaDto>> RegistrarRango(RangoAsistenciaDto dto);
    Task<VacacionesAnioDto> ObtenerVacaciones(int idEmpleado, int? anio);
    Task Eliminar(int idAsistencia);
}
