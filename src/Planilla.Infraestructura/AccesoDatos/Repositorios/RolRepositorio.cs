using Microsoft.EntityFrameworkCore;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;

namespace Planilla.Infraestructura.AccesoDatos.Repositorios;

public class RolRepositorio : IRolRepositorio
{
    private readonly PlanillaDbContext _context;

    public RolRepositorio(PlanillaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Rol>> ObtenerTodos()
    {
        return await _context.Rol
            .AsNoTracking()
            .OrderBy(r => r.Id)
            .ToListAsync();
    }

    public async Task<Rol> ObtenerPorId(int id)
    {
        return await _context.Rol
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> ExisteNombre(string nombre)
    {
        return await _context.Rol.AnyAsync(r => r.Nombre == nombre);
    }

    public async Task Agregar(Rol rol)
    {
        _context.Rol.Add(rol);
        await _context.SaveChangesAsync();
    }
}
