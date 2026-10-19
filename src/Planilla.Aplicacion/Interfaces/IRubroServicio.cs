using Planilla.Aplicacion.DTOs;

namespace Planilla.Aplicacion.Interfaces;

public interface IRubroServicio
{
    Task<List<RubroDto>> ObtenerTodos(bool soloActivos, string tipo);
    Task<RubroDto> ObtenerPorId(int idRubro);
    Task<RubroDto> Crear(RubroCrearDto dto);
}
