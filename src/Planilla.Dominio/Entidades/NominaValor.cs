using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planilla.Dominio.Entidades;

[Index("IdNominaEmpleado", "IdRubro", Name = "UQ_NominaValor_Empleado_Rubro", IsUnique = true)]
public partial class NominaValor
{
    [Key]
    public int IdNominaValor { get; set; }

    public int IdNominaEmpleado { get; set; }

    public int IdRubro { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
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
    [InverseProperty("NominaValor")]
    public virtual NominaEmpleado IdNominaEmpleadoNavigation { get; set; }

    [ForeignKey("IdRubro")]
    [InverseProperty("NominaValor")]
    public virtual Rubro IdRubroNavigation { get; set; }
}
