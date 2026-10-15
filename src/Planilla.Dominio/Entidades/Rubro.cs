using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planilla.Dominio.Entidades;

[Index("Nombre", Name = "UQ_Rubro_Nombre", IsUnique = true)]
public partial class Rubro
{
    [Key]
    public int IdRubro { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; }

    [Required]
    [StringLength(10)]
    public string Tipo { get; set; }

    public bool AfectoIgss { get; set; }

    [StringLength(200)]
    public string Descripcion { get; set; }

    [StringLength(20)]
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
    public virtual ICollection<MovimientoNomina> MovimientoNomina { get; set; } = new List<MovimientoNomina>();
}
