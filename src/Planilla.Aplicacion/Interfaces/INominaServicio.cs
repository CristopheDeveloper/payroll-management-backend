using Planilla.Aplicacion.DTOs;

namespace Planilla.Aplicacion.Interfaces;

public interface INominaServicio
{
    Task<List<TipoPlanillaDto>> ObtenerTiposPlanilla(bool soloActivos);
    Task<List<NominaDto>> ObtenerTodas(int? anio, int? idTipoPlanilla);
    Task<NominaDto> ObtenerPorId(int idNomina);
    Task<NominaDto> Crear(NominaCrearDto dto);
    Task<NominaDto> Editar(int idNomina, NominaEditarDto dto);
    Task Eliminar(int idNomina);
    Task<NominaDto> Preparar(int idNomina);
    Task<List<NominaEmpleadoDto>> ObtenerEmpleados(int idNomina);
    Task<NominaEmpleadoDto> GuardarDatosEmpleado(int idNomina, int idEmpleado, DatosEmpleadoGuardarDto dto);
    Task<List<NominaEmpleadoDto>> GuardarVariable(int idNomina, VariableGuardarDto dto);
    Task<List<NominaEmpleadoDto>> GuardarVariables(int idNomina, ImportarVariablesDto dto);
    Task<NominaDto> Generar(int idNomina);
    Task<NominaDto> VolverAPreparada(int idNomina);
    Task<List<ResumenRubroDto>> ObtenerResumenRubros(int idNomina);
    Task<NominaDto> Cerrar(int idNomina, CerrarDto dto);
    Task<NominaDto> Reabrir(int idNomina, ReabrirDto dto);
    Task<List<PagoEmpleadoDto>> ObtenerPagosEmpleado(int idEmpleado);
}
