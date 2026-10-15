using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Interfaces;

public interface IDepartamentoRepositorio
{
    Task<List<Departamento>> ObtenerTodos(bool soloActivos);
    Task<Departamento> ObtenerPorId(int idDepartamento);
    Task<Departamento> ObtenerParaModificar(int idDepartamento);
    Task<bool> ExisteNombre(string nombre, int idExcluir);
    void Agregar(Departamento departamento);
}
