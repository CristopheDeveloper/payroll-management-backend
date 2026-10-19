using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Interfaces;

public interface IRubroRepositorio
{
    Task<List<Rubro>> ObtenerTodos(bool soloActivos, string tipo);
    Task<Rubro> ObtenerPorId(int idRubro);
    Task<bool> ExisteNombre(string nombre);
    void Agregar(Rubro rubro);
}
