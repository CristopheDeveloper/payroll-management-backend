namespace Planilla.Aplicacion.DTOs;

public class RubroDto
{
    public int IdRubro { get; set; }
    public string Codigo { get; set; }
    public string Nombre { get; set; }
    public string Tipo { get; set; }
    public string Origen { get; set; }
    public bool AfectoIgss { get; set; }
    public bool EsPrestacion { get; set; }
    public int Orden { get; set; }
    public string Descripcion { get; set; }
    public string CuentaContable { get; set; }
    public bool Activo { get; set; }
}
