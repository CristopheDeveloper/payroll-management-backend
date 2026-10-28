namespace Planilla.Aplicacion.DTOs;

public class ReporteTablaDto
{
    public string Titulo { get; set; }
    public string Subtitulo { get; set; }
    public List<string> Filtros { get; set; }
    public string NombreEmpresa { get; set; }
    public string NitEmpresa { get; set; }
    public string DireccionEmpresa { get; set; }
    public DateTime FechaGeneracion { get; set; }
    public bool Preliminar { get; set; }
    public string NombreArchivo { get; set; }
    public List<ColumnaReporteDto> Columnas { get; set; }
    public List<FilaReporteDto> Filas { get; set; }
}
