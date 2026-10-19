using Microsoft.EntityFrameworkCore;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;

namespace Planilla.Infraestructura.AccesoDatos.Repositorios;

public class ParametroRepositorio : IParametroRepositorio
{
    private readonly PlanillaDbContext _context;

    public ParametroRepositorio(PlanillaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Parametro>> ObtenerTodos()
    {
        return await _context.Parametro
            .AsNoTracking()
            .OrderBy(p => p.IdParametro)
            .ToListAsync();
    }

    public async Task<Parametro> ObtenerPorId(int idParametro)
    {
        return await _context.Parametro
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdParametro == idParametro);
    }

    public async Task<Parametro> ObtenerPorClave(string clave)
    {
        return await _context.Parametro
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Clave == clave);
    }

    public async Task<Parametro> ObtenerParaModificar(int idParametro)
    {
        return await _context.Parametro.FindAsync(idParametro);
    }
}
