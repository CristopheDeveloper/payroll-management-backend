using Microsoft.EntityFrameworkCore;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;

namespace Planilla.Infraestructura.AccesoDatos.Repositorios;

public class RubroRepositorio : IRubroRepositorio
{
    private readonly PlanillaDbContext _context;

    public RubroRepositorio(PlanillaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Rubro>> ObtenerTodos(bool soloActivos, string tipo)
    {
        return await _context.Rubro
            .AsNoTracking()
            .Where(r => (!soloActivos || r.Activo) && (tipo == null || r.Tipo == tipo))
            .OrderBy(r => r.Tipo)
            .ThenBy(r => r.Nombre)
            .ToListAsync();
    }

    public async Task<Rubro> ObtenerPorId(int idRubro)
    {
        return await _context.Rubro
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IdRubro == idRubro);
    }

    public async Task<bool> ExisteNombre(string nombre)
    {
        return await _context.Rubro.AnyAsync(r => r.Nombre == nombre);
    }

    public void Agregar(Rubro rubro)
    {
        _context.Rubro.Add(rubro);
    }
}
