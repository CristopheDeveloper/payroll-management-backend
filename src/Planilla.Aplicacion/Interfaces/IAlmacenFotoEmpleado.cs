using Planilla.Aplicacion.DTOs;

namespace Planilla.Aplicacion.Interfaces;

public interface IAlmacenFotoEmpleado
{
    Task<ArchivoFotoGuardadoDto> Guardar(Stream contenido, string extension, CancellationToken cancellationToken = default);
    Task<Stream> Abrir(string rutaSftp, CancellationToken cancellationToken = default);
    Task Eliminar(string rutaSftp, CancellationToken cancellationToken = default);
}
