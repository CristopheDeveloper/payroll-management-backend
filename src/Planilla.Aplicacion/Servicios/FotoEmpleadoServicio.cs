using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Excepciones;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Servicios;

public class FotoEmpleadoServicio : IFotoEmpleadoServicio
{
    public const long TamanoMaximo = 5 * 1024 * 1024;

    private readonly IEmpleadoRepositorio _empleadoRepositorio;
    private readonly IAlmacenFotoEmpleado _almacen;
    private readonly IUnidadTrabajo _unidadTrabajo;

    public FotoEmpleadoServicio(IEmpleadoRepositorio empleadoRepositorio, IAlmacenFotoEmpleado almacen,
        IUnidadTrabajo unidadTrabajo)
    {
        _empleadoRepositorio = empleadoRepositorio;
        _almacen = almacen;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<EmpleadoFotoDto> Guardar(int idEmpleado, Stream contenido, string nombreArchivo,
        string tipoContenido, long tamanoBytes, CancellationToken cancellationToken = default)
    {
        _ = nombreArchivo;
        _ = tipoContenido;
        if (await _empleadoRepositorio.ObtenerParaModificar(idEmpleado) == null)
            throw new ReglaNegocioException("El empleado no existe.", 404);
        if (tamanoBytes <= 0)
            throw new ReglaNegocioException("Selecciona una fotografía con contenido.");
        if (tamanoBytes > TamanoMaximo)
            throw new ReglaNegocioException("La fotografía no puede superar 5 MB.", 413);

        var (extension, tipoReal) = await DetectarFormato(contenido, cancellationToken);
        var anterior = await _empleadoRepositorio.ObtenerFotoParaModificar(idEmpleado);
        var guardado = await _almacen.Guardar(contenido, extension, cancellationToken);

        var rutaAnterior = anterior?.RutaSftp;
        try
        {
            if (anterior == null)
            {
                anterior = new EmpleadoFoto { IdEmpleado = idEmpleado };
                _empleadoRepositorio.AgregarFoto(anterior);
            }

            anterior.Url = $"/api/Empleado/{idEmpleado}/foto/contenido?v={Guid.NewGuid():N}";
            anterior.RutaSftp = guardado.RutaSftp;
            anterior.NombreArchivo = guardado.NombreArchivo;
            anterior.TipoContenido = tipoReal;
            anterior.TamanoBytes = tamanoBytes;
            await _unidadTrabajo.GuardarCambiosAsync();
        }
        catch
        {
            try { await _almacen.Eliminar(guardado.RutaSftp, CancellationToken.None); }
            catch { /* Se conserva la excepción original de base de datos. */ }
            throw;
        }

        if (!string.IsNullOrWhiteSpace(rutaAnterior) && rutaAnterior != guardado.RutaSftp)
        {
            try { await _almacen.Eliminar(rutaAnterior, cancellationToken); }
            catch { /* La foto vigente ya quedó guardada; el archivo anterior puede limpiarse después. */ }
        }

        return ADto(anterior);
    }

    public async Task<(Stream Contenido, string TipoContenido)> Obtener(int idEmpleado,
        CancellationToken cancellationToken = default)
    {
        var foto = await _empleadoRepositorio.ObtenerFotoParaModificar(idEmpleado)
            ?? throw new ReglaNegocioException("El empleado no tiene fotografía de perfil.", 404);
        try
        {
            var contenido = await _almacen.Abrir(foto.RutaSftp, cancellationToken);
            return (contenido, foto.TipoContenido);
        }
        catch (FileNotFoundException)
        {
            throw new ReglaNegocioException("La fotografía no está disponible en el servidor de archivos.", 404);
        }
    }

    public async Task Eliminar(int idEmpleado, CancellationToken cancellationToken = default)
    {
        if (await _empleadoRepositorio.ObtenerParaModificar(idEmpleado) == null)
            throw new ReglaNegocioException("El empleado no existe.", 404);

        var foto = await _empleadoRepositorio.ObtenerFotoParaModificar(idEmpleado);
        if (foto == null) return;

        _empleadoRepositorio.EliminarFoto(foto);
        await _unidadTrabajo.GuardarCambiosAsync();
        try { await _almacen.Eliminar(foto.RutaSftp, cancellationToken); }
        catch { /* La relación ya no existe; no se deja una URL rota en la aplicación. */ }
    }

    private static async Task<(string Extension, string TipoContenido)> DetectarFormato(
        Stream contenido, CancellationToken cancellationToken)
    {
        if (!contenido.CanSeek)
            throw new ReglaNegocioException("No fue posible leer la fotografía.");

        var cabecera = new byte[12];
        var leidos = await contenido.ReadAsync(cabecera.AsMemory(0, cabecera.Length), cancellationToken);
        contenido.Position = 0;

        if (leidos >= 3 && cabecera[0] == 0xFF && cabecera[1] == 0xD8 && cabecera[2] == 0xFF)
            return (".jpg", "image/jpeg");
        if (leidos >= 8 && cabecera.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            return (".png", "image/png");
        if (leidos >= 12 && cabecera.AsSpan(0, 4).SequenceEqual("RIFF"u8)
            && cabecera.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            return (".webp", "image/webp");

        throw new ReglaNegocioException("Formato no permitido. Usa una imagen JPG, PNG o WebP.");
    }

    private static EmpleadoFotoDto ADto(EmpleadoFoto foto) => new()
    {
        Url = foto.Url,
        NombreArchivo = foto.NombreArchivo,
        TipoContenido = foto.TipoContenido,
        TamanoBytes = foto.TamanoBytes
    };
}
