using Planilla.Aplicacion.DTOs;

namespace Planilla.Aplicacion.Interfaces;

public interface IPuestoServicio
{
    Task<List<PuestoDto>> ObtenerTodos(bool soloActivos);
    Task<PuestoDto> ObtenerPorId(int idPuesto);
    Task<PuestoDto> Crear(PuestoGuardarDto dto);
    Task<PuestoDto> Editar(int idPuesto, PuestoGuardarDto dto);
    Task Activar(int idPuesto);
    Task Desactivar(int idPuesto);
}
