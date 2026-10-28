using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planilla.Dominio.Entidades;

[Index("Codigo", Name = "UQ_TipoPlanilla_Codigo", IsUnique = true)]
[Index("Nombre", Name = "UQ_TipoPlanilla_Nombre", IsUnique = true)]
public partial class TipoPlanilla
{
    [Key]
    public int IdTipoPlanilla { get; set; }

    [Required]
    [StringLength(20)]
    public string Codigo { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; }

    [StringLength(200)]
    public string Descripcion { get; set; }

    public bool IncluyeIgss { get; set; }

    public bool Activo { get; set; }

    [Required]
    [StringLength(50)]
    public string UsuarioIng { get; set; }

    public DateTime FechaIng { get; set; }

    [StringLength(50)]
    public string UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }

    [InverseProperty("IdTipoPlanillaNavigation")]
    public virtual ICollection<Nomina> Nomina { get; set; } = new List<Nomina>();
}
