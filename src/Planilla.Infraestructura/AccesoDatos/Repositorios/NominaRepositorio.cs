using Microsoft.EntityFrameworkCore;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;

namespace Planilla.Infraestructura.AccesoDatos.Repositorios;

public class NominaRepositorio : INominaRepositorio
{
    private readonly PlanillaDbContext _context;

    public NominaRepositorio(PlanillaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Nomina>> ObtenerTodas(int? anio)
    {
        return await _context.Nomina
            .AsNoTracking()
            .Where(n => anio == null || n.Anio == anio)
            .OrderByDescending(n => n.Anio)
            .ThenByDescending(n => n.Mes)
            .ToListAsync();
    }

    public async Task<Dictionary<int, int>> ContarEmpleados(List<int> idsNomina)
    {
        return await _context.NominaDetalle
            .Where(d => idsNomina.Contains(d.IdNomina))
            .GroupBy(d => d.IdNomina)
            .Select(g => new { IdNomina = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.IdNomina, x => x.Cantidad);
    }

    public async Task<Nomina> ObtenerPorId(int idNomina)
    {
        return await _context.Nomina
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.IdNomina == idNomina);
    }

    public async Task<Nomina> ObtenerParaModificar(int idNomina)
    {
        return await _context.Nomina.FindAsync(idNomina);
    }

    public async Task<bool> Existe(int anio, int mes)
    {
        return await _context.Nomina.AnyAsync(n => n.Anio == anio && n.Mes == mes);
    }

    public async Task<DateOnly?> ObtenerInicioMasAntiguo()
    {
        return await _context.Nomina.MinAsync(n => (DateOnly?)n.FechaInicio);
    }

    public async Task<List<Nomina>> ObtenerCerradasEntre(DateOnly desde, DateOnly hasta)
    {
        return await _context.Nomina
            .AsNoTracking()
            .Where(n => n.Estado == "CERRADA" && n.FechaInicio <= hasta && n.FechaFin >= desde)
            .OrderBy(n => n.FechaInicio)
            .ToListAsync();
    }

    public async Task<List<PeriodoLaboral>> ObtenerPeriodosEntre(DateOnly desde, DateOnly hasta)
    {
        return await _context.PeriodoLaboral
            .AsNoTracking()
            .Where(p => p.FechaIngreso <= hasta && (p.FechaBaja == null || p.FechaBaja >= desde))
            .ToListAsync();
    }

    public async Task<bool> TienePeriodoEntre(int idEmpleado, DateOnly desde, DateOnly hasta)
    {
        return await _context.PeriodoLaboral
            .AnyAsync(p => p.IdEmpleado == idEmpleado && p.FechaIngreso <= hasta && (p.FechaBaja == null || p.FechaBaja >= desde));
    }

    public async Task<List<Empleado>> ObtenerEmpleados(List<int> idsEmpleado)
    {
        return await _context.Empleado
            .AsNoTracking()
            .Where(e => idsEmpleado.Contains(e.IdEmpleado))
            .ToListAsync();
    }

    public async Task<List<NominaDetalle>> ObtenerDetalle(int idNomina)
    {
        return await _context.NominaDetalle
            .AsNoTracking()
            .Include(d => d.IdEmpleadoNavigation)
            .Include(d => d.IdPuestoNavigation)
            .Include(d => d.IdDepartamentoNavigation)
            .Where(d => d.IdNomina == idNomina)
            .OrderBy(d => d.IdEmpleadoNavigation.Apellidos)
            .ThenBy(d => d.IdEmpleadoNavigation.Nombres)
            .ToListAsync();
    }

    public async Task<List<NominaDetalle>> ObtenerDetalleParaModificar(int idNomina)
    {
        return await _context.NominaDetalle
            .Where(d => d.IdNomina == idNomina)
            .ToListAsync();
    }

    public async Task<List<MovimientoNomina>> ObtenerMovimientos(int idNomina, int? idEmpleado)
    {
        return await _context.MovimientoNomina
            .AsNoTracking()
            .Include(m => m.IdEmpleadoNavigation)
            .Include(m => m.IdRubroNavigation)
            .Where(m => m.IdNomina == idNomina && (idEmpleado == null || m.IdEmpleado == idEmpleado))
            .OrderBy(m => m.IdMovimiento)
            .ToListAsync();
    }

    public async Task<List<MovimientoNomina>> ObtenerMovimientosParaModificar(int idNomina)
    {
        return await _context.MovimientoNomina
            .Where(m => m.IdNomina == idNomina)
            .ToListAsync();
    }

    public async Task<MovimientoNomina> ObtenerMovimiento(int idMovimiento)
    {
        return await _context.MovimientoNomina
            .AsNoTracking()
            .Include(m => m.IdEmpleadoNavigation)
            .Include(m => m.IdRubroNavigation)
            .FirstOrDefaultAsync(m => m.IdMovimiento == idMovimiento);
    }

    public async Task<MovimientoNomina> ObtenerMovimientoParaModificar(int idMovimiento)
    {
        return await _context.MovimientoNomina.FindAsync(idMovimiento);
    }

    public void Agregar(Nomina nomina)
    {
        _context.Nomina.Add(nomina);
    }

    public void Eliminar(Nomina nomina)
    {
        _context.Nomina.Remove(nomina);
    }

    public void AgregarDetalle(NominaDetalle fila)
    {
        _context.NominaDetalle.Add(fila);
    }

    public void EliminarDetalle(NominaDetalle fila)
    {
        _context.NominaDetalle.Remove(fila);
    }

    public void AgregarMovimiento(MovimientoNomina movimiento)
    {
        _context.MovimientoNomina.Add(movimiento);
    }

    public void EliminarMovimiento(MovimientoNomina movimiento)
    {
        _context.MovimientoNomina.Remove(movimiento);
    }

    public void MarcarModificada(Nomina nomina)
    {
        _context.Entry(nomina).Property(n => n.FechaAct).IsModified = true;
    }

    public void FijarVersion(Nomina nomina, byte[] version)
    {
        _context.Entry(nomina).Property(n => n.Version).OriginalValue = version;
    }
}
