namespace Planilla.Dominio.Calculo;

public class ValorManual
{
    public int IdRubro { get; set; }
    public string Tipo { get; set; }
    public bool AfectoIgss { get; set; }
    public int Orden { get; set; }
    public decimal Monto { get; set; }
    public string Descripcion { get; set; }
}
