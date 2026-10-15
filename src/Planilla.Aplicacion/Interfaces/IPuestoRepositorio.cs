using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Interfaces;

public interface IPuestoRepositorio
{
    Task<List<Puesto>> ObtenerTodos(bool soloActivos);
    Task<Puesto> ObtenerPorId(int idPuesto);
    Task<Puesto> ObtenerParaModificar(int idPuesto);
    Task<bool> ExisteNombre(string nombre, int idExcluir);
    void Agregar(Puesto puesto);
}
