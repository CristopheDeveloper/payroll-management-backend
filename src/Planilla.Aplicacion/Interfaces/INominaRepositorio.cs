using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Interfaces;

public interface INominaRepositorio
{
    Task<List<Nomina>> ObtenerTodas(int? anio, int? idTipoPlanilla);
    Task<Dictionary<int, int>> ContarEmpleados(List<int> idsNomina);
    Task<Nomina> ObtenerPorId(int idNomina);
    Task<Nomina> ObtenerParaModificar(int idNomina);
    Task<bool> ExisteTipoMes(int idTipoPlanilla, int anio, int mes);
    Task<List<string>> ObtenerCodigosDelMes(int idTipoPlanilla, int anio, int mes, int excluirId);
    Task<bool> ExisteCodigo(string codigo, int? excluirId);
    Task<DateOnly?> ObtenerInicioMasAntiguo(int idTipoPlanilla);
    Task<List<Nomina>> ObtenerCerradasEntre(DateOnly desde, DateOnly hasta);
    Task<List<TipoPlanilla>> ObtenerTiposPlanilla(bool soloActivos);
    Task<TipoPlanilla> ObtenerTipoPlanilla(int idTipoPlanilla);
    Task<List<PeriodoLaboral>> ObtenerPeriodosEntre(DateOnly desde, DateOnly hasta);
    Task<List<Empleado>> ObtenerEmpleados(List<int> idsEmpleado);
    Task<List<NominaEmpleado>> ObtenerEmpleadosNomina(int idNomina);
    Task<List<NominaEmpleado>> ObtenerEmpleadosNominaParaModificar(int idNomina);
    Task<List<NominaEmpleado>> ObtenerPagosEmpleado(int idEmpleado);
    void Agregar(Nomina nomina);
    void Eliminar(Nomina nomina);
    void AgregarEmpleado(NominaEmpleado fila);
    void EliminarEmpleado(NominaEmpleado fila);
    void AgregarValor(NominaValor valor);
    void EliminarValor(NominaValor valor);
    void AgregarRubro(NominaRubro rubro);
    void EliminarRubro(NominaRubro rubro);
    void FijarVersion(Nomina nomina, byte[] version);
}
