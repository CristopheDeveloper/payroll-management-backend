using Microsoft.EntityFrameworkCore;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;

namespace Planilla.Infraestructura.AccesoDatos.Repositorios;

public class EmpleadoRepositorio : IEmpleadoRepositorio
{
    private readonly PlanillaDbContext _context;

    public EmpleadoRepositorio(PlanillaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Empleado>> ObtenerTodos(bool? activo, int? idDepartamento, int? idPuesto, string buscar)
    {
        var consulta = _context.Empleado
            .AsNoTracking()
            .Include(e => e.IdPuestoNavigation)
            .Include(e => e.IdDepartamentoNavigation)
            .Include(e => e.PeriodoLaboral)
            .Include(e => e.Foto)
            .AsQueryable();

        if (activo.HasValue)
        {
            consulta = consulta.Where(e => e.Activo == activo.Value);
        }
        if (idDepartamento.HasValue)
        {
            consulta = consulta.Where(e => e.IdDepartamento == idDepartamento.Value);
        }
        if (idPuesto.HasValue)
        {
            consulta = consulta.Where(e => e.IdPuesto == idPuesto.Value);
        }
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            consulta = consulta.Where(e => e.CodigoEmpleado.Contains(texto)
                || e.Dpi.Contains(texto)
                || e.Nombres.Contains(texto)
                || e.Apellidos.Contains(texto)
                || (e.Nombres + " " + e.Apellidos).Contains(texto));
        }

        return await consulta
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<Empleado> ObtenerPorId(int idEmpleado)
    {
        return await _context.Empleado
            .AsNoTracking()
            .Include(e => e.IdPuestoNavigation)
            .Include(e => e.IdDepartamentoNavigation)
            .Include(e => e.PeriodoLaboral)
            .Include(e => e.Foto)
            .FirstOrDefaultAsync(e => e.IdEmpleado == idEmpleado);
    }

    public async Task<Empleado> ObtenerParaModificar(int idEmpleado)
    {
        return await _context.Empleado.FindAsync(idEmpleado);
    }

    public async Task<Empleado> ObtenerPorDpi(string dpi, int idExcluir)
    {
        return await _context.Empleado
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Dpi == dpi && e.IdEmpleado != idExcluir);
    }

    public async Task<EmpleadoFoto> ObtenerFotoParaModificar(int idEmpleado)
    {
        return await _context.EmpleadoFoto.FirstOrDefaultAsync(f => f.IdEmpleado == idEmpleado);
    }

    public async Task<List<PeriodoLaboral>> ObtenerPeriodosParaModificar(int idEmpleado)
    {
        return await _context.PeriodoLaboral
            .Where(p => p.IdEmpleado == idEmpleado)
            .OrderByDescending(p => p.FechaIngreso)
            .ThenByDescending(p => p.IdPeriodoLaboral)
            .ToListAsync();
    }

    public async Task<bool> ExistenActivosEnDepartamento(int idDepartamento)
    {
        return await _context.Empleado.AnyAsync(e => e.Activo && e.IdDepartamento == idDepartamento);
    }

    public async Task<bool> ExistenActivosEnPuesto(int idPuesto)
    {
        return await _context.Empleado.AnyAsync(e => e.Activo && e.IdPuesto == idPuesto);
    }

    public void Agregar(Empleado empleado)
    {
        _context.Empleado.Add(empleado);
    }

    public void AgregarPeriodo(PeriodoLaboral periodo)
    {
        _context.PeriodoLaboral.Add(periodo);
    }

    public void AgregarFoto(EmpleadoFoto foto)
    {
        _context.EmpleadoFoto.Add(foto);
    }

    public void EliminarFoto(EmpleadoFoto foto)
    {
        _context.EmpleadoFoto.Remove(foto);
    }
}
