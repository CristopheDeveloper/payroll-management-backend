using Planilla.Aplicacion.DTOs;

namespace Planilla.Aplicacion.Interfaces;

public interface IParametroServicio
{
    Task<List<ParametroDto>> ObtenerTodos();
    Task<ParametroDto> ObtenerPorId(int idParametro);
    Task<decimal> ObtenerSalarioMinimo();
    Task<decimal> ObtenerBonificacionIncentivo();
    Task<ParametroDto> Editar(int idParametro, ParametroEditarDto dto);
}
