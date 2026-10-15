using Microsoft.EntityFrameworkCore;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;

namespace Planilla.Infraestructura.AccesoDatos.Repositorios;

public class DepartamentoRepositorio : IDepartamentoRepositorio
{
    private readonly PlanillaDbContext _context;

    public DepartamentoRepositorio(PlanillaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Departamento>> ObtenerTodos(bool soloActivos)
    {
        return await _context.Departamento
            .AsNoTracking()
            .Where(d => !soloActivos || d.Activo)
            .OrderBy(d => d.Nombre)
            .ToListAsync();
    }

    public async Task<Departamento> ObtenerPorId(int idDepartamento)
    {
        return await _context.Departamento
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.IdDepartamento == idDepartamento);
    }

    public async Task<Departamento> ObtenerParaModificar(int idDepartamento)
    {
        return await _context.Departamento.FindAsync(idDepartamento);
    }

    public async Task<bool> ExisteNombre(string nombre, int idExcluir)
    {
        return await _context.Departamento
            .AnyAsync(d => d.Nombre == nombre && d.IdDepartamento != idExcluir);
    }

    public void Agregar(Departamento departamento)
    {
        _context.Departamento.Add(departamento);
    }
}
