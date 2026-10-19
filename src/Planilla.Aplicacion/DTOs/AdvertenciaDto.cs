namespace Planilla.Aplicacion.DTOs;

public class AdvertenciaDto
{
    public string Codigo { get; set; }
    public string Mensaje { get; set; }
    public bool Bloquea { get; set; }
    public int? IdEmpleado { get; set; }
}
