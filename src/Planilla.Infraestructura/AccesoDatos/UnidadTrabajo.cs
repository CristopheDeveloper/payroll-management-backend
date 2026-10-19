using Planilla.Aplicacion.Interfaces;

namespace Planilla.Infraestructura.AccesoDatos;

public class UnidadTrabajo : IUnidadTrabajo
{
    private readonly PlanillaDbContext _context;
    private readonly Auditoria _auditoria;
    private readonly RegistroBitacora _bitacora;

    public UnidadTrabajo(PlanillaDbContext context, Auditoria auditoria, RegistroBitacora bitacora)
    {
        _context = context;
        _auditoria = auditoria;
        _bitacora = bitacora;
    }

    public async Task GuardarCambiosAsync()
    {
        _auditoria.LlenarCampos(_context.ChangeTracker);
        var cambios = _bitacora.Recolectar(_context.ChangeTracker);

        await using var transaccion = await _context.Database.BeginTransactionAsync();

        await _context.SaveChangesAsync();

        if (cambios.Count > 0)
        {
            _bitacora.Registrar(cambios);
            await _context.SaveChangesAsync();
        }

        await transaccion.CommitAsync();
    }
}
