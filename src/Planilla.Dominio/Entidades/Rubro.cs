using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planilla.Dominio.Entidades;

[Index("Codigo", Name = "UQ_Rubro_Codigo", IsUnique = true)]
[Index("Nombre", Name = "UQ_Rubro_Nombre", IsUnique = true)]
public partial class Rubro
{
    [Key]
    public int IdRubro { get; set; }

    [Required]
    [StringLength(30)]
    public string Codigo { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; }

    [Required]
    [StringLength(10)]
    public string Tipo { get; set; }

    [Required]
    [StringLength(10)]
    public string Origen { get; set; }

    public bool AfectoIgss { get; set; }

    public bool EsPrestacion { get; set; }

    public int Orden { get; set; }

    [StringLength(200)]
    public string Descripcion { get; set; }

    [StringLength(4)]
    [Unicode(false)]
    public string CuentaContable { get; set; }

    public bool Activo { get; set; }

    [Required]
    [StringLength(50)]
    public string UsuarioIng { get; set; }

    public DateTime FechaIng { get; set; }

    [StringLength(50)]
    public string UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }

    [InverseProperty("IdRubroNavigation")]
    public virtual ICollection<NominaRubro> NominaRubro { get; set; } = new List<NominaRubro>();

    [InverseProperty("IdRubroNavigation")]
    public virtual ICollection<NominaValor> NominaValor { get; set; } = new List<NominaValor>();
}
