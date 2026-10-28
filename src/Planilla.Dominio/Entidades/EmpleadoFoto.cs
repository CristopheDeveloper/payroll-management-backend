using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Planilla.Dominio.Entidades;

public partial class EmpleadoFoto
{
    [Key]
    [ForeignKey(nameof(Empleado))]
    public int IdEmpleado { get; set; }

    [Required]
    [StringLength(2048)]
    public string Url { get; set; }

    [Required]
    [StringLength(500)]
    public string RutaSftp { get; set; }

    [Required]
    [StringLength(255)]
    public string NombreArchivo { get; set; }

    [Required]
    [StringLength(100)]
    public string TipoContenido { get; set; }

    public long TamanoBytes { get; set; }

    [Required]
    [StringLength(50)]
    public string UsuarioIng { get; set; }

    public DateTime FechaIng { get; set; }

    [StringLength(50)]
    public string UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }

    public virtual Empleado Empleado { get; set; }
}
