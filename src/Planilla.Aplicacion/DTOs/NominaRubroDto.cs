namespace Planilla.Aplicacion.DTOs;

public class NominaRubroDto
{
    public int IdRubro { get; set; }
    public string Codigo { get; set; }
    public string Nombre { get; set; }
    public string Tipo { get; set; }
    public int Orden { get; set; }
    public decimal? Base { get; set; }
    public decimal Monto { get; set; }
    public string Descripcion { get; set; }
}
