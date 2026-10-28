namespace Planilla.Aplicacion.DTOs;

public class ResumenAsistenciaDto
{
    public int IdEmpleado { get; set; }
    public string CodigoEmpleado { get; set; }
    public string NombreCompleto { get; set; }
    public int Faltas { get; set; }
    public int Permisos { get; set; }
    public int SuspensionIgss { get; set; }
    public decimal HorasExtra { get; set; }
}
