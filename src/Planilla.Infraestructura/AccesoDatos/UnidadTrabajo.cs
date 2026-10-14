using Planilla.Aplicacion.Interfaces;

namespace Planilla.Infraestructura.AccesoDatos;

public class UnidadTrabajo : IUnidadTrabajo
{
    private readonly PlanillaDbContext _context;
    private readonly Auditoria _auditoria;

    public UnidadTrabajo(PlanillaDbContext context, Auditoria auditoria)
    {
        _context = context;
        _auditoria = auditoria;
    }

    public async Task GuardarCambiosAsync()
    {
        _auditoria.LlenarCampos(_context.ChangeTracker);
        await _context.SaveChangesAsync();
    }
}
