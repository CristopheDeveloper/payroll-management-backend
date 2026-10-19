namespace Planilla.Aplicacion.DTOs;

public class DepartamentoReporteDto
{
    public string NombreDepartamento { get; set; }
    public List<FilaPlanillaDto> Filas { get; set; }
    public TotalesReporteDto Subtotal { get; set; }
}
