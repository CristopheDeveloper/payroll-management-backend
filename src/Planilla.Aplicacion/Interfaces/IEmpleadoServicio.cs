using Planilla.Aplicacion.DTOs;

namespace Planilla.Aplicacion.Interfaces;

public interface IEmpleadoServicio
{
    Task<List<EmpleadoDto>> ObtenerTodos(bool? activo, int? idDepartamento, int? idPuesto, string buscar);
    Task<EmpleadoDetalleDto> ObtenerPorId(int idEmpleado);
    Task<EmpleadoDetalleDto> Contratar(EmpleadoCrearDto dto);
    Task<EmpleadoDetalleDto> Editar(int idEmpleado, EmpleadoEditarDto dto);
    Task<EmpleadoDetalleDto> DarDeBaja(int idEmpleado, BajaDto dto);
    Task<EmpleadoDetalleDto> Reingresar(int idEmpleado, ReingresoDto dto);
    Task<EmpleadoDetalleDto> CorregirFechaIngreso(int idEmpleado, CorregirFechaIngresoDto dto);
}
