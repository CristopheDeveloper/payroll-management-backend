using Planilla.Aplicacion.DTOs;

namespace Planilla.Aplicacion.Interfaces;

public interface IDepartamentoServicio
{
    Task<List<DepartamentoDto>> ObtenerTodos(bool soloActivos);
    Task<DepartamentoDto> ObtenerPorId(int idDepartamento);
    Task<DepartamentoDto> Crear(DepartamentoGuardarDto dto);
    Task<DepartamentoDto> Editar(int idDepartamento, DepartamentoGuardarDto dto);
    Task Activar(int idDepartamento);
    Task Desactivar(int idDepartamento);
}
