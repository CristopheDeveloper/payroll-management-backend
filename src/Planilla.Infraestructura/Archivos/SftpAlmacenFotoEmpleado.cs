using Microsoft.Extensions.Options;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Interfaces;
using Renci.SshNet;

namespace Planilla.Infraestructura.Archivos;

public class SftpAlmacenFotoEmpleado : IAlmacenFotoEmpleado
{
    private readonly SftpFotoEmpleadoOpciones _opciones;

    public SftpAlmacenFotoEmpleado(IOptions<SftpFotoEmpleadoOpciones> opciones)
    {
        _opciones = opciones.Value;
    }

    public Task<ArchivoFotoGuardadoDto> Guardar(Stream contenido, string extension,
        CancellationToken cancellationToken = default)
    {
        ValidarConfiguracion();
        var nombre = $"{Guid.NewGuid():N}{extension}";
        var directorio = NormalizarDirectorio(_opciones.RemoteDirectory);
        var ruta = $"{directorio}/{nombre}";

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var cliente = CrearCliente();
            cliente.Connect();
            CrearDirectorioSiNoExiste(cliente, directorio);
            cliente.UploadFile(contenido, ruta, true);
            cliente.Disconnect();

            return new ArchivoFotoGuardadoDto
            {
                NombreArchivo = nombre,
                RutaSftp = ruta
            };
        }, cancellationToken);
    }

    public Task<Stream> Abrir(string rutaSftp, CancellationToken cancellationToken = default)
    {
        ValidarConfiguracion();
        return Task.Run<Stream>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var cliente = CrearCliente();
            cliente.Connect();
            if (!cliente.Exists(rutaSftp))
                throw new FileNotFoundException("La fotografía no existe en el servidor SFTP.", rutaSftp);
            var contenido = new MemoryStream();
            cliente.DownloadFile(rutaSftp, contenido);
            contenido.Position = 0;
            cliente.Disconnect();
            return contenido;
        }, cancellationToken);
    }

    public Task Eliminar(string rutaSftp, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rutaSftp)) return Task.CompletedTask;
        ValidarConfiguracion();

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var cliente = CrearCliente();
            cliente.Connect();
            if (cliente.Exists(rutaSftp)) cliente.DeleteFile(rutaSftp);
            cliente.Disconnect();
        }, cancellationToken);
    }

    private SftpClient CrearCliente()
    {
        var cliente = new SftpClient(_opciones.Host, _opciones.Port, _opciones.Username, _opciones.Password);
        if (!string.IsNullOrWhiteSpace(_opciones.HostKeyFingerprint))
        {
            var esperado = LimpiarHuella(_opciones.HostKeyFingerprint);
            cliente.HostKeyReceived += (_, evento) =>
                evento.CanTrust = string.Equals(Convert.ToHexString(evento.FingerPrint), esperado,
                    StringComparison.OrdinalIgnoreCase);
        }
        return cliente;
    }

    private static void CrearDirectorioSiNoExiste(SftpClient cliente, string directorio)
    {
        var actual = directorio.StartsWith('/') ? "/" : string.Empty;
        foreach (var parte in directorio.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            actual = string.IsNullOrEmpty(actual) || actual == "/" ? $"{actual}{parte}" : $"{actual}/{parte}";
            if (!cliente.Exists(actual)) cliente.CreateDirectory(actual);
        }
    }

    private void ValidarConfiguracion()
    {
        if (string.IsNullOrWhiteSpace(_opciones.Host)
            || string.IsNullOrWhiteSpace(_opciones.Username)
            || string.IsNullOrWhiteSpace(_opciones.Password))
        {
            throw new InvalidOperationException(
                "Falta configurar EmployeePhotos:Host, Username o Password.");
        }
    }

    private static string NormalizarDirectorio(string directorio)
    {
        var valor = (directorio ?? "imagenes").Trim().Replace('\\', '/').TrimEnd('/');
        return string.IsNullOrWhiteSpace(valor) ? "imagenes" : valor;
    }

    private static string LimpiarHuella(string huella) =>
        huella.Replace(":", string.Empty).Replace("-", string.Empty).Trim();
}
