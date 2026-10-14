using Microsoft.EntityFrameworkCore;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;

namespace Planilla.Infraestructura.AccesoDatos.Repositorios;

public class PuestoRepositorio : IPuestoRepositorio
{
    private readonly PlanillaDbContext _context;

    public PuestoRepositorio(PlanillaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Puesto>> ObtenerTodos(bool soloActivos)
    {
        return await _context.Puesto
            .AsNoTracking()
            .Where(p => !soloActivos || p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<Puesto> ObtenerPorId(int idPuesto)
    {
        return await _context.Puesto
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPuesto == idPuesto);
    }

    public async Task<Puesto> ObtenerParaModificar(int idPuesto)
    {
        return await _context.Puesto.FindAsync(idPuesto);
    }

    public async Task<bool> ExisteNombre(string nombre, int idExcluir)
    {
        return await _context.Puesto
            .AnyAsync(p => p.Nombre == nombre && p.IdPuesto != idExcluir);
    }

    public void Agregar(Puesto puesto)
    {
        _context.Puesto.Add(puesto);
    }
}
