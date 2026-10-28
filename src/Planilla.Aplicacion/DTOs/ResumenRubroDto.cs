namespace Planilla.Aplicacion.DTOs;

public class ResumenRubroDto
{
    public int IdRubro { get; set; }
    public string Codigo { get; set; }
    public string Nombre { get; set; }
    public string Tipo { get; set; }
    public int Orden { get; set; }
    public int Empleados { get; set; }
    public decimal Total { get; set; }
}
