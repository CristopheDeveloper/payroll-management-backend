using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Interfaces;

public interface IParametroRepositorio
{
    Task<List<Parametro>> ObtenerTodos();
    Task<Parametro> ObtenerPorId(int idParametro);
    Task<Parametro> ObtenerPorClave(string clave);
    Task<Parametro> ObtenerParaModificar(int idParametro);
}
