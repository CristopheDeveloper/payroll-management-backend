namespace Planilla.Aplicacion.DTOs;

public class PeriodoLaboralDto
{
    public int IdPeriodoLaboral { get; set; }
    public DateOnly FechaIngreso { get; set; }
    public DateOnly? FechaBaja { get; set; }
    public string TipoBaja { get; set; }
    public string MotivoBaja { get; set; }
}
