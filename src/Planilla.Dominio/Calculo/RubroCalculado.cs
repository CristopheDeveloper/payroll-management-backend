namespace Planilla.Dominio.Calculo;

public class RubroCalculado
{
    public string Codigo { get; set; }
    public int? IdRubro { get; set; }
    public string Tipo { get; set; }
    public decimal? Base { get; set; }
    public decimal Monto { get; set; }
    public string Descripcion { get; set; }
}
