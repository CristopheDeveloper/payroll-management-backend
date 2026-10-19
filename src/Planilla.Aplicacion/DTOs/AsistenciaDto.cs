namespace Planilla.Aplicacion.DTOs;

public class AsistenciaDto
{
    public int IdAsistencia { get; set; }
    public DateOnly Fecha { get; set; }
    public string Estado { get; set; }
    public decimal HorasExtra { get; set; }
    public string Observacion { get; set; }
}
