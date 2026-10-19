namespace Planilla.Aplicacion.DTOs;

public class AsistenciaDiaDto
{
    public int IdEmpleado { get; set; }
    public string CodigoEmpleado { get; set; }
    public string NombreCompleto { get; set; }
    public string Departamento { get; set; }
    public int? IdAsistencia { get; set; }
    public string Estado { get; set; }
    public decimal HorasExtra { get; set; }
    public string Observacion { get; set; }
}
