using Microsoft.EntityFrameworkCore;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;

namespace Planilla.Infraestructura.AccesoDatos.Repositorios;

public class AsistenciaRepositorio : IAsistenciaRepositorio
{
    private readonly PlanillaDbContext _context;

    public AsistenciaRepositorio(PlanillaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Empleado>> ObtenerEmpleadosConPeriodoEntre(DateOnly desde, DateOnly hasta)
    {
        return await _context.Empleado
            .AsNoTracking()
            .Include(e => e.IdDepartamentoNavigation)
            .Where(e => e.PeriodoLaboral.Any(p => p.FechaIngreso <= hasta && (p.FechaBaja == null || p.FechaBaja >= desde)))
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .ToListAsync();
    }

    public async Task<List<Asistencia>> ObtenerPorFecha(DateOnly fecha)
    {
        return await _context.Asistencia
            .AsNoTracking()
            .Where(a => a.Fecha == fecha)
            .ToListAsync();
    }

    public async Task<List<Asistencia>> ObtenerPorFechaParaModificar(DateOnly fecha)
    {
        return await _context.Asistencia
            .Where(a => a.Fecha == fecha)
            .ToListAsync();
    }

    public async Task<List<Asistencia>> ObtenerPorEmpleado(int idEmpleado, DateOnly desde, DateOnly hasta)
    {
        return await _context.Asistencia
            .AsNoTracking()
            .Where(a => a.IdEmpleado == idEmpleado && a.Fecha >= desde && a.Fecha <= hasta)
            .OrderBy(a => a.Fecha)
            .ToListAsync();
    }

    public async Task<List<ResumenAsistenciaDto>> ObtenerTotales(DateOnly desde, DateOnly hasta)
    {
        return await _context.Asistencia
            .AsNoTracking()
            .Where(a => a.Fecha >= desde && a.Fecha <= hasta
                && a.IdEmpleadoNavigation.PeriodoLaboral.Any(p => p.FechaIngreso <= a.Fecha && (p.FechaBaja == null || p.FechaBaja >= a.Fecha)))
            .GroupBy(a => a.IdEmpleado)
            .Select(g => new ResumenAsistenciaDto
            {
                IdEmpleado = g.Key,
                Faltas = g.Count(a => a.Estado == "FALTA"),
                Permisos = g.Count(a => a.Estado == "PERMISO"),
                Vacaciones = g.Count(a => a.Estado == "VACACIONES"),
                SuspensionIgss = g.Count(a => a.Estado == "SUSPENSION_IGSS"),
                HorasExtra = g.Sum(a => a.HorasExtra)
            })
            .ToListAsync();
    }

    public async Task<List<DateOnly>> ObtenerFechas(int idEmpleado, DateOnly desde, DateOnly? hasta)
    {
        return await _context.Asistencia
            .Where(a => a.IdEmpleado == idEmpleado && a.Fecha >= desde && (hasta == null || a.Fecha <= hasta))
            .OrderBy(a => a.Fecha)
            .Select(a => a.Fecha)
            .ToListAsync();
    }

    public async Task<Asistencia> ObtenerParaModificar(int idAsistencia)
    {
        return await _context.Asistencia.FindAsync(idAsistencia);
    }

    public void Agregar(Asistencia asistencia)
    {
        _context.Asistencia.Add(asistencia);
    }

    public void Eliminar(Asistencia asistencia)
    {
        _context.Asistencia.Remove(asistencia);
    }
}
