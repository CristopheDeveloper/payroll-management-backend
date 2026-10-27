using Microsoft.EntityFrameworkCore;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;
using Planilla.Dominio.Tiempo;

namespace Planilla.Infraestructura.AccesoDatos.Repositorios;

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly PlanillaDbContext _context;

    public UsuarioRepositorio(PlanillaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Usuario>> ObtenerTodos()
    {
        return await _context.Usuario
            .AsNoTracking()
            .Include(u => u.IdRolNavigation)
            .OrderBy(u => u.NombreUsuario)
            .ToListAsync();
    }

    public async Task<Usuario> ObtenerPorId(int idUsuario)
    {
        return await _context.Usuario
            .AsNoTracking()
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
    }

    public async Task<Usuario> ObtenerParaModificar(int idUsuario)
    {
        return await _context.Usuario.FindAsync(idUsuario);
    }

    public async Task<Usuario> ObtenerPorNombreUsuario(string nombreUsuario)
    {
        return await _context.Usuario
            .AsNoTracking()
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
    }

    public async Task<bool> ExisteNombreUsuario(string nombreUsuario)
    {
        return await _context.Usuario.AnyAsync(u => u.NombreUsuario == nombreUsuario);
    }

    public async Task<int> ContarActivosPorRol(string rol)
    {
        return await _context.Usuario
            .CountAsync(u => u.Activo && u.IdRolNavigation.Nombre == rol);
    }

    public async Task<Rol> ObtenerRol(int idRol)
    {
        return await _context.Rol
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IdRol == idRol);
    }

    public async Task<List<Rol>> ObtenerRolesActivos()
    {
        return await _context.Rol
            .AsNoTracking()
            .Where(r => r.Activo)
            .OrderBy(r => r.IdRol)
            .ToListAsync();
    }

    public void Agregar(Usuario usuario)
    {
        _context.Usuario.Add(usuario);
    }

    public async Task ActualizarUltimoAcceso(int idUsuario)
    {
        var ahora = RelojGuatemala.Ahora;
        await _context.Usuario
            .Where(u => u.IdUsuario == idUsuario)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.UltimoAcceso, ahora));
    }
}
