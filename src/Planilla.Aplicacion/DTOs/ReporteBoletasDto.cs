namespace Planilla.Aplicacion.DTOs;

public class ReporteBoletasDto
{
    public EncabezadoReporteDto Encabezado { get; set; }
    public List<BoletaDto> Boletas { get; set; }
}
