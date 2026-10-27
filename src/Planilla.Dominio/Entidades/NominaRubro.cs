using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planilla.Dominio.Entidades;

[Index("IdNominaEmpleado", "Orden", Name = "IX_NominaRubro_NominaEmpleado_Orden")]
[Index("IdNominaEmpleado", "IdRubro", Name = "UQ_NominaRubro_Empleado_Rubro", IsUnique = true)]
public partial class NominaRubro
{
    [Key]
    public int IdNominaRubro { get; set; }

    public int IdNominaEmpleado { get; set; }

    public int IdRubro { get; set; }

    public int Orden { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? Base { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal Monto { get; set; }

    [StringLength(200)]
    public string Descripcion { get; set; }

    [Required]
    [StringLength(50)]
    public string UsuarioIng { get; set; }

    public DateTime FechaIng { get; set; }

    [StringLength(50)]
    public string UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }

    [ForeignKey("IdNominaEmpleado")]
    [InverseProperty("NominaRubro")]
    public virtual NominaEmpleado IdNominaEmpleadoNavigation { get; set; }

    [ForeignKey("IdRubro")]
    [InverseProperty("NominaRubro")]
    public virtual Rubro IdRubroNavigation { get; set; }
}
