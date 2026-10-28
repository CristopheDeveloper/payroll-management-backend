namespace Planilla.Infraestructura.Archivos;

public class SftpFotoEmpleadoOpciones
{
    public const string Seccion = "EmployeePhotos";

    public string Host { get; set; }
    public int Port { get; set; } = 22;
    public string Username { get; set; } = "fileserver";
    public string Password { get; set; }
    public string RemoteDirectory { get; set; } = "imagenes";
    public string HostKeyFingerprint { get; set; }
}
