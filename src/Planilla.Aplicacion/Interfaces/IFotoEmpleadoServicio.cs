using Planilla.Aplicacion.DTOs;

namespace Planilla.Aplicacion.Interfaces;

public interface IFotoEmpleadoServicio
{
    Task<EmpleadoFotoDto> Guardar(int idEmpleado, Stream contenido, string nombreArchivo,
        string tipoContenido, long tamanoBytes, CancellationToken cancellationToken = default);
    Task<(Stream Contenido, string TipoContenido)> Obtener(int idEmpleado,
        CancellationToken cancellationToken = default);
    Task Eliminar(int idEmpleado, CancellationToken cancellationToken = default);
}
