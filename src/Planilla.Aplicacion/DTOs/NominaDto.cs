namespace Planilla.Aplicacion.DTOs;

public class NominaDto
{
    public int IdNomina { get; set; }
    public int Anio { get; set; }
    public int Mes { get; set; }
    public string Estado { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal TotalLiquido { get; set; }
    public decimal TotalPatronal { get; set; }
    public int CantidadEmpleados { get; set; }
    public DateTime? FechaCalculo { get; set; }
    public decimal? IgssLaboral { get; set; }
    public decimal? IgssPatronal { get; set; }
    public decimal? Irtra { get; set; }
    public decimal? Intecap { get; set; }
    public decimal? BonificacionIncentivo { get; set; }
    public decimal? FactorHoraExtra { get; set; }
    public int? DiasMes { get; set; }
    public int? HorasMes { get; set; }
    public string UsuarioCierre { get; set; }
    public DateTime? FechaCierre { get; set; }
    public string MotivoReapertura { get; set; }
    public string Version { get; set; }
    public bool? PuedeCerrar { get; set; }
    public List<AdvertenciaDto> Advertencias { get; set; }
}
