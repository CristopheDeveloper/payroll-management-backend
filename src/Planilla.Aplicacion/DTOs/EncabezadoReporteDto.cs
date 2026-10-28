namespace Planilla.Aplicacion.DTOs;

public class EncabezadoReporteDto
{
    public int IdNomina { get; set; }
    public string Codigo { get; set; }
    public string Descripcion { get; set; }
    public string NombreEmpresa { get; set; }
    public string NitEmpresa { get; set; }
    public string DireccionEmpresa { get; set; }
    public int Anio { get; set; }
    public int Mes { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public string Estado { get; set; }
    public bool Preliminar { get; set; }
    public DateTime? FechaCierre { get; set; }
    public string UsuarioCierre { get; set; }
}
