namespace Planilla.Aplicacion.DTOs;

public class HistorialSalarioDto
{
    public DateTime Fecha { get; set; }
    public string Usuario { get; set; }
    public decimal? SalarioAnterior { get; set; }
    public decimal SalarioNuevo { get; set; }
}
