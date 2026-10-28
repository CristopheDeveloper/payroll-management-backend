using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Interfaces;

public interface IEmpleadoRepositorio
{
    Task<List<Empleado>> ObtenerTodos(bool? activo, int? idDepartamento, int? idPuesto, string buscar);
    Task<Empleado> ObtenerPorId(int idEmpleado);
    Task<Empleado> ObtenerParaModificar(int idEmpleado);
    Task<Empleado> ObtenerPorDpi(string dpi, int idExcluir);
    Task<EmpleadoFoto> ObtenerFotoParaModificar(int idEmpleado);
    Task<List<PeriodoLaboral>> ObtenerPeriodosParaModificar(int idEmpleado);
    Task<bool> ExistenActivosEnDepartamento(int idDepartamento);
    Task<bool> ExistenActivosEnPuesto(int idPuesto);
    void Agregar(Empleado empleado);
    void AgregarPeriodo(PeriodoLaboral periodo);
    void AgregarFoto(EmpleadoFoto foto);
    void EliminarFoto(EmpleadoFoto foto);
}
