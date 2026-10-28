using Planilla.Aplicacion.DTOs;
using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Interfaces;

public interface IAsistenciaRepositorio
{
    Task<List<Empleado>> ObtenerEmpleadosConPeriodoEntre(DateOnly desde, DateOnly hasta);
    Task<List<Asistencia>> ObtenerPorFecha(DateOnly fecha);
    Task<List<Asistencia>> ObtenerPorFechaParaModificar(DateOnly fecha);
    Task<List<Asistencia>> ObtenerPorEmpleado(int idEmpleado, DateOnly desde, DateOnly hasta);
    Task<List<Asistencia>> ObtenerPorEmpleadoParaModificar(int idEmpleado, DateOnly desde, DateOnly hasta);
    Task<List<ResumenAsistenciaDto>> ObtenerTotales(DateOnly desde, DateOnly hasta);
    Task<List<Asistencia>> ObtenerAusenciasEntre(DateOnly desde, DateOnly hasta);
    Task<List<DateOnly>> ObtenerFechas(int idEmpleado, DateOnly desde, DateOnly? hasta);
    Task<Asistencia> ObtenerParaModificar(int idAsistencia);
    void Agregar(Asistencia asistencia);
    void Eliminar(Asistencia asistencia);
}
