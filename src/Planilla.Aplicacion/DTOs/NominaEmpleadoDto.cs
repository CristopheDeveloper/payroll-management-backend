namespace Planilla.Aplicacion.DTOs;

public class NominaEmpleadoDto
{
    public int IdNominaEmpleado { get; set; }
    public int IdEmpleado { get; set; }
    public string CodigoEmpleado { get; set; }
    public string NombreCompleto { get; set; }
    public int IdPuesto { get; set; }
    public string NombrePuesto { get; set; }
    public int IdDepartamento { get; set; }
    public string NombreDepartamento { get; set; }
    public decimal Salario { get; set; }
    public decimal RetencionIsr { get; set; }
    public string TipoPago { get; set; }
    public string Banco { get; set; }
    public string CuentaBancaria { get; set; }
    public int DiasPeriodo { get; set; }
    public int DiasFalta { get; set; }
    public int DiasSuspension { get; set; }
    public int DiasPagarSugeridos { get; set; }
    public int DiasPagar { get; set; }
    public decimal HorasExtraSugeridas { get; set; }
    public decimal HorasExtra { get; set; }
    public decimal Isr { get; set; }
    public bool DatosAjustados { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal Liquido { get; set; }
    public decimal TotalPatronal { get; set; }
    public List<NominaValorDto> Valores { get; set; }
    public List<NominaRubroDto> Rubros { get; set; }
    public List<AdvertenciaDto> Advertencias { get; set; }
}
