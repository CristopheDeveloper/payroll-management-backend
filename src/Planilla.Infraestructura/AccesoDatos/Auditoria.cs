using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Auditoria;
using Planilla.Dominio.Tiempo;

namespace Planilla.Infraestructura.AccesoDatos;

public class Auditoria
{
    private readonly IUsuarioActual _usuarioActual;

    public Auditoria(IUsuarioActual usuarioActual)
    {
        _usuarioActual = usuarioActual;
    }

    public void LlenarCampos(ChangeTracker changeTracker)
    {
        var usuario = _usuarioActual.NombreUsuario;
        var ahora = RelojGuatemala.Ahora;

        foreach (var entry in changeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.UsuarioIng = usuario;
                entry.Entity.FechaIng = ahora;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UsuarioAct = usuario;
                entry.Entity.FechaAct = ahora;

                entry.Property(nameof(IAuditable.UsuarioIng)).IsModified = false;
                entry.Property(nameof(IAuditable.FechaIng)).IsModified = false;
            }
        }
    }
}
