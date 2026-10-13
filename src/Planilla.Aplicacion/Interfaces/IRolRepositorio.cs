using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Interfaces;

public interface IRolRepositorio
{
    Task<List<Rol>> ObtenerTodos();
    Task<Rol> ObtenerPorId(int id);
    Task<bool> ExisteNombre(string nombre);
    Task Agregar(Rol rol);
}
