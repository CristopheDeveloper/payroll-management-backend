using Planilla.Aplicacion.DTOs;

namespace Planilla.Aplicacion.Interfaces;

public interface IMovimientoNominaServicio
{
    Task<List<MovimientoDto>> ObtenerTodos(int idNomina, int? idEmpleado);
    Task<MovimientoDto> Crear(int idNomina, MovimientoGuardarDto dto);
    Task<MovimientoDto> Editar(int idNomina, int idMovimiento, MovimientoGuardarDto dto);
    Task Eliminar(int idNomina, int idMovimiento);
}
