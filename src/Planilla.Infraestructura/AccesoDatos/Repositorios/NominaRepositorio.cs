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

    public async Task<List<Nomina>> ObtenerTodas(int? anio, int? idTipoPlanilla)
    {
        return await _context.Nomina
            .AsNoTracking()
            .Include(n => n.IdTipoPlanillaNavigation)
            .Where(n => (anio == null || n.Anio == anio) && (idTipoPlanilla == null || n.IdTipoPlanilla == idTipoPlanilla))
            .OrderByDescending(n => n.Anio)
            .ThenByDescending(n => n.Mes)
            .ThenBy(n => n.IdTipoPlanilla)
            .ToListAsync();
    }

    public async Task<Dictionary<int, int>> ContarEmpleados(List<int> idsNomina)
    {
        return await _context.NominaEmpleado
            .Where(e => idsNomina.Contains(e.IdNomina))
            .GroupBy(e => e.IdNomina)
            .Select(g => new { IdNomina = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.IdNomina, x => x.Cantidad);
    }

    public async Task<Nomina> ObtenerPorId(int idNomina)
    {
        return await _context.Nomina
            .AsNoTracking()
            .Include(n => n.IdTipoPlanillaNavigation)
            .FirstOrDefaultAsync(n => n.IdNomina == idNomina);
    }

    public async Task<Nomina> ObtenerParaModificar(int idNomina)
    {
        return await _context.Nomina
            .Include(n => n.IdTipoPlanillaNavigation)
            .FirstOrDefaultAsync(n => n.IdNomina == idNomina);
    }

    public async Task<bool> ExisteTipoMes(int idTipoPlanilla, int anio, int mes)
    {
        return await _context.Nomina.AnyAsync(n => n.IdTipoPlanilla == idTipoPlanilla && n.Anio == anio && n.Mes == mes);
    }

    public async Task<List<string>> ObtenerCodigosDelMes(int idTipoPlanilla, int anio, int mes, int excluirId)
    {
        return await _context.Nomina
            .Where(n => n.IdTipoPlanilla == idTipoPlanilla && n.Anio == anio && n.Mes == mes && n.IdNomina != excluirId)
            .OrderBy(n => n.Codigo)
            .Select(n => n.Codigo)
            .ToListAsync();
    }

    public async Task<bool> ExisteCodigo(string codigo, int? excluirId)
    {
        return await _context.Nomina.AnyAsync(n => n.Codigo == codigo && (excluirId == null || n.IdNomina != excluirId));
    }

    public async Task<DateOnly?> ObtenerInicioMasAntiguo(int idTipoPlanilla)
    {
        return await _context.Nomina
            .Where(n => n.IdTipoPlanilla == idTipoPlanilla)
            .MinAsync(n => (DateOnly?)n.FechaInicio);
    }

    public async Task<List<Nomina>> ObtenerCerradasEntre(DateOnly desde, DateOnly hasta)
    {
        return await _context.Nomina
            .AsNoTracking()
            .Where(n => n.Estado == "CERRADA" && n.FechaInicio <= hasta && n.FechaFin >= desde)
            .OrderBy(n => n.FechaInicio)
            .ToListAsync();
    }

    public async Task<List<TipoPlanilla>> ObtenerTiposPlanilla(bool soloActivos)
    {
        return await _context.TipoPlanilla
            .AsNoTracking()
            .Where(t => !soloActivos || t.Activo)
            .OrderBy(t => t.IdTipoPlanilla)
            .ToListAsync();
    }

    public async Task<TipoPlanilla> ObtenerTipoPlanilla(int idTipoPlanilla)
    {
        return await _context.TipoPlanilla
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IdTipoPlanilla == idTipoPlanilla);
    }

    public async Task<List<PeriodoLaboral>> ObtenerPeriodosEntre(DateOnly desde, DateOnly hasta)
    {
        return await _context.PeriodoLaboral
            .AsNoTracking()
            .Where(p => p.FechaIngreso <= hasta && (p.FechaBaja == null || p.FechaBaja >= desde))
            .ToListAsync();
    }

    public async Task<List<Empleado>> ObtenerEmpleados(List<int> idsEmpleado)
    {
        return await _context.Empleado
            .AsNoTracking()
            .Where(e => idsEmpleado.Contains(e.IdEmpleado))
            .ToListAsync();
    }

    public async Task<List<NominaEmpleado>> ObtenerEmpleadosNomina(int idNomina)
    {
        return await _context.NominaEmpleado
            .AsNoTracking()
            .AsSplitQuery()
            .Include(e => e.IdEmpleadoNavigation)
            .Include(e => e.IdPuestoNavigation)
            .Include(e => e.IdDepartamentoNavigation)
            .Include(e => e.NominaValor).ThenInclude(v => v.IdRubroNavigation)
            .Include(e => e.NominaRubro).ThenInclude(r => r.IdRubroNavigation)
            .Where(e => e.IdNomina == idNomina)
            .OrderBy(e => e.IdDepartamentoNavigation.Nombre)
            .ThenBy(e => e.IdEmpleadoNavigation.Apellidos)
            .ThenBy(e => e.IdEmpleadoNavigation.Nombres)
            .ToListAsync();
    }

    public async Task<List<NominaEmpleado>> ObtenerEmpleadosNominaParaModificar(int idNomina)
    {
        return await _context.NominaEmpleado
            .AsSplitQuery()
            .Include(e => e.IdEmpleadoNavigation)
            .Include(e => e.NominaValor).ThenInclude(v => v.IdRubroNavigation)
            .Include(e => e.NominaRubro)
            .Where(e => e.IdNomina == idNomina)
            .ToListAsync();
    }

    public async Task<List<NominaEmpleado>> ObtenerPagosEmpleado(int idEmpleado)
    {
        return await _context.NominaEmpleado
            .AsNoTracking()
            .AsSplitQuery()
            .Include(e => e.IdNominaNavigation).ThenInclude(n => n.IdTipoPlanillaNavigation)
            .Include(e => e.NominaRubro).ThenInclude(r => r.IdRubroNavigation)
            .Where(e => e.IdEmpleado == idEmpleado && e.IdNominaNavigation.Estado == "CERRADA")
            .OrderByDescending(e => e.IdNominaNavigation.FechaInicio)
            .ToListAsync();
    }

    public void Agregar(Nomina nomina)
    {
        _context.Nomina.Add(nomina);
    }

    public void Eliminar(Nomina nomina)
    {
        _context.Nomina.Remove(nomina);
    }

    public void AgregarEmpleado(NominaEmpleado fila)
    {
        _context.NominaEmpleado.Add(fila);
    }

    public void EliminarEmpleado(NominaEmpleado fila)
    {
        _context.NominaEmpleado.Remove(fila);
    }

    public void AgregarValor(NominaValor valor)
    {
        _context.NominaValor.Add(valor);
    }

    public void EliminarValor(NominaValor valor)
    {
        _context.NominaValor.Remove(valor);
    }

    public void AgregarRubro(NominaRubro rubro)
    {
        _context.NominaRubro.Add(rubro);
    }

    public void EliminarRubro(NominaRubro rubro)
    {
        _context.NominaRubro.Remove(rubro);
    }

    public void FijarVersion(Nomina nomina, byte[] version)
    {
        _context.Entry(nomina).Property(n => n.Version).OriginalValue = version;
    }
}
