using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Interfaces;

public interface IRubroRepositorio
{
    Task<List<Rubro>> ObtenerTodos(bool soloActivos, string tipo, string origen);
    Task<Rubro> ObtenerPorId(int idRubro);
    Task<Rubro> ObtenerParaModificar(int idRubro);
    Task<bool> ExisteNombre(string nombre, int? excluirId);
    Task<bool> ExisteCodigo(string codigo);
    void Agregar(Rubro rubro);
}
