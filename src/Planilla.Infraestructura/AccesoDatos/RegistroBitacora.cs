using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;

namespace Planilla.Infraestructura.AccesoDatos;

// Escribe en Bitacora cada INSERT, UPDATE y DELETE. UnidadTrabajo la usa en dos pasos:
// Recolectar antes de guardar (todavía se conocen los valores anteriores) y
// Registrar después de guardar (SQL Server ya generó los Ids de los INSERT).
public class RegistroBitacora
{
    // Los campos de auditoría no cuentan como cambios
    private static readonly HashSet<string> CamposExcluidos =
    [
        "UsuarioIng", "FechaIng", "UsuarioAct", "FechaAct"
    ];

    // Deja las tildes y la ñ legibles en el JSON
    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly PlanillaDbContext _context;
    private readonly IUsuarioActual _usuarioActual;

    public RegistroBitacora(PlanillaDbContext context, IUsuarioActual usuarioActual)
    {
        _context = context;
        _usuarioActual = usuarioActual;
    }

    public List<CambioBitacora> Recolectar(ChangeTracker changeTracker)
    {
        var cambios = new List<CambioBitacora>();

        foreach (var entry in changeTracker.Entries())
        {
            if (entry.Entity is Bitacora)
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    // El Id y los valores se leen en Registrar, cuando ya existen
                    cambios.Add(new CambioBitacora(entry, "INSERT"));
                    break;

                case EntityState.Deleted:
                    cambios.Add(new CambioBitacora(entry, "DELETE")
                    {
                        IdRegistro = LeerId(entry),
                        ValoresAnteriores = TodosLosValores(entry, originales: true)
                    });
                    break;

                case EntityState.Modified:
                    var anteriores = new Dictionary<string, object>();
                    var nuevos = new Dictionary<string, object>();

                    foreach (var prop in entry.Properties)
                    {
                        var campo = prop.Metadata.Name;
                        if (CamposExcluidos.Contains(campo)
                            || !prop.IsModified
                            || Equals(prop.OriginalValue, prop.CurrentValue))
                        {
                            continue;
                        }

                        anteriores[campo] = prop.OriginalValue;
                        nuevos[campo] = prop.CurrentValue;
                    }

                    // Si solo cambiaron campos excluidos, no hay nada que registrar
                    if (anteriores.Count > 0)
                    {
                        cambios.Add(new CambioBitacora(entry, "UPDATE")
                        {
                            IdRegistro = LeerId(entry),
                            ValoresAnteriores = anteriores,
                            ValoresNuevos = nuevos
                        });
                    }
                    break;
            }
        }

        return cambios;
    }

    public void Registrar(List<CambioBitacora> cambios)
    {
        var usuario = _usuarioActual.NombreUsuario;
        var ahora = DateTime.Now;

        foreach (var cambio in cambios)
        {
            if (cambio.Accion == "INSERT")
            {
                cambio.IdRegistro = LeerId(cambio.Entry);
                cambio.ValoresNuevos = TodosLosValores(cambio.Entry, originales: false);
            }

            _context.Bitacora.Add(new Bitacora
            {
                Tabla = cambio.Tabla,
                IdRegistro = cambio.IdRegistro,
                Accion = cambio.Accion,
                ValoresAnteriores = AJson(cambio.ValoresAnteriores),
                ValoresNuevos = AJson(cambio.ValoresNuevos),
                Usuario = usuario,
                Fecha = ahora
            });
        }
    }

    // Todas las tablas tienen una llave primaria INT de una sola columna (Id<Tabla>)
    private static int LeerId(EntityEntry entry)
    {
        var llave = entry.Metadata.FindPrimaryKey().Properties[0].Name;
        return Convert.ToInt32(entry.Property(llave).CurrentValue);
    }

    private static Dictionary<string, object> TodosLosValores(EntityEntry entry, bool originales)
    {
        var valores = new Dictionary<string, object>();
        foreach (var prop in entry.Properties)
        {
            if (!CamposExcluidos.Contains(prop.Metadata.Name))
            {
                valores[prop.Metadata.Name] = originales ? prop.OriginalValue : prop.CurrentValue;
            }
        }
        return valores;
    }

    private static string AJson(Dictionary<string, object> valores)
    {
        return valores == null ? null : JsonSerializer.Serialize(valores, OpcionesJson);
    }
}

public class CambioBitacora
{
    public CambioBitacora(EntityEntry entry, string accion)
    {
        Entry = entry;
        Accion = accion;
        Tabla = entry.Metadata.GetTableName();
    }

    public EntityEntry Entry { get; }
    public string Accion { get; }
    public string Tabla { get; }
    public int IdRegistro { get; set; }
    public Dictionary<string, object> ValoresAnteriores { get; set; }
    public Dictionary<string, object> ValoresNuevos { get; set; }
}
