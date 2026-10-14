using Microsoft.EntityFrameworkCore;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;

namespace Planilla.Infraestructura.AccesoDatos.Repositorios;

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly PlanillaDbContext _context;

    public UsuarioRepositorio(PlanillaDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario> ObtenerPorId(int idUsuario)
    {
        return await _context.Usuario
            .AsNoTracking()
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
    }

    public async Task<Usuario> ObtenerPorNombreUsuario(string nombreUsuario)
    {
        return await _context.Usuario
            .AsNoTracking()
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
    }

    public async Task ActualizarUltimoAcceso(int idUsuario)
    {
        await _context.Usuario
            .Where(u => u.IdUsuario == idUsuario)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.UltimoAcceso, DateTime.Now));
    }
}
