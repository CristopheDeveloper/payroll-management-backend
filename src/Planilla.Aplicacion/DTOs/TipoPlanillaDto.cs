namespace Planilla.Aplicacion.DTOs;

public class TipoPlanillaDto
{
    public int IdTipoPlanilla { get; set; }
    public string Codigo { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public bool IncluyeIgss { get; set; }
    public bool Activo { get; set; }
}
