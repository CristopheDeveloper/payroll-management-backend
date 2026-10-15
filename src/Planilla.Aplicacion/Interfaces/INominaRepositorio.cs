using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Interfaces;

public interface INominaRepositorio
{
    Task<List<Nomina>> ObtenerTodas(int? anio);
    Task<Dictionary<int, int>> ContarEmpleados(List<int> idsNomina);
    Task<Nomina> ObtenerPorId(int idNomina);
    Task<Nomina> ObtenerParaModificar(int idNomina);
    Task<bool> Existe(int anio, int mes);
    Task<DateOnly?> ObtenerInicioMasAntiguo();
    Task<List<Nomina>> ObtenerCerradasEntre(DateOnly desde, DateOnly hasta);
    Task<List<PeriodoLaboral>> ObtenerPeriodosEntre(DateOnly desde, DateOnly hasta);
    Task<bool> TienePeriodoEntre(int idEmpleado, DateOnly desde, DateOnly hasta);
    Task<List<Empleado>> ObtenerEmpleados(List<int> idsEmpleado);
    Task<List<NominaDetalle>> ObtenerDetalle(int idNomina);
    Task<List<NominaDetalle>> ObtenerDetalleParaModificar(int idNomina);
    Task<List<MovimientoNomina>> ObtenerMovimientos(int idNomina, int? idEmpleado);
    Task<List<MovimientoNomina>> ObtenerMovimientosParaModificar(int idNomina);
    Task<MovimientoNomina> ObtenerMovimiento(int idMovimiento);
    Task<MovimientoNomina> ObtenerMovimientoParaModificar(int idMovimiento);
    void Agregar(Nomina nomina);
    void Eliminar(Nomina nomina);
    void AgregarDetalle(NominaDetalle fila);
    void EliminarDetalle(NominaDetalle fila);
    void AgregarMovimiento(MovimientoNomina movimiento);
    void EliminarMovimiento(MovimientoNomina movimiento);
    void MarcarModificada(Nomina nomina);
    void FijarVersion(Nomina nomina, byte[] version);
}
