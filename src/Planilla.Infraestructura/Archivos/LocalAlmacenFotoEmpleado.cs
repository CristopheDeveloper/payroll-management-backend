using Microsoft.Extensions.Options;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Interfaces;

namespace Planilla.Infraestructura.Archivos;

public class LocalAlmacenFotoEmpleado : IAlmacenFotoEmpleado
{
    private readonly string _directorio;

    public LocalAlmacenFotoEmpleado(IOptions<SftpFotoEmpleadoOpciones> opciones)
    {
        _directorio = ResolverDirectorio(opciones.Value.LocalDirectory);
        Directory.CreateDirectory(_directorio);
    }

    public async Task<ArchivoFotoGuardadoDto> Guardar(Stream contenido, string extension,
        CancellationToken cancellationToken = default)
    {
        var nombre = $"{Guid.NewGuid():N}{extension}";
        var ruta = RutaSegura(nombre);

        await using var archivo = new FileStream(ruta, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            81920, FileOptions.Asynchronous);
        await contenido.CopyToAsync(archivo, cancellationToken);

        return new ArchivoFotoGuardadoDto
        {
            NombreArchivo = nombre,
            RutaSftp = nombre
        };
    }

    public Task<Stream> Abrir(string rutaSftp, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var ruta = RutaSegura(rutaSftp);
        Stream contenido = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.Read,
            81920, FileOptions.Asynchronous);
        return Task.FromResult(contenido);
    }

    public Task Eliminar(string rutaSftp, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rutaSftp)) return Task.CompletedTask;

        cancellationToken.ThrowIfCancellationRequested();
        var ruta = RutaSegura(rutaSftp);
        if (File.Exists(ruta)) File.Delete(ruta);
        return Task.CompletedTask;
    }

    private string RutaSegura(string referencia)
    {
        if (string.IsNullOrWhiteSpace(referencia) || referencia != Path.GetFileName(referencia))
            throw new FileNotFoundException("La fotografía local no existe.", referencia);

        return Path.Combine(_directorio, referencia);
    }

    private static string ResolverDirectorio(string configurado)
    {
        if (!string.IsNullOrWhiteSpace(configurado))
            return Path.GetFullPath(configurado);

        var documentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (string.IsNullOrWhiteSpace(documentos))
            throw new InvalidOperationException("No fue posible localizar la carpeta Documentos del usuario.");

        return Path.Combine(documentos, "Planilla", "imagenes");
    }
}
