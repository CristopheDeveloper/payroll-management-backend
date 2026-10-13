using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Interfaces;

public interface IRolServicio
{
    Task<List<Rol>> ObtenerTodos();
    Task<Rol> ObtenerPorId(int id);
    Task<Rol> Crear(Rol rol);
}
