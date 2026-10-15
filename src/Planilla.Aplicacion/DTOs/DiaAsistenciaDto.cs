namespace Planilla.Aplicacion.DTOs;

public class DiaAsistenciaDto
{
    public DateOnly Fecha { get; set; }
    public string Version { get; set; }
    public List<AsistenciaDiaDto> Empleados { get; set; }
}
