using Planilla.Aplicacion.DTOs;

namespace Planilla.Aplicacion.Interfaces;

public interface IRubroServicio
{
    Task<List<RubroDto>> ObtenerTodos(bool soloActivos, string tipo, string origen);
    Task<RubroDto> ObtenerPorId(int idRubro);
    Task<RubroDto> Crear(RubroCrearDto dto);
    Task<RubroDto> Editar(int idRubro, RubroEditarDto dto);
    Task CambiarEstado(int idRubro, bool activo);
}
