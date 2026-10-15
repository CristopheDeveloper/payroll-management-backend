using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planilla.Dominio.Entidades;

[Index("IdNomina", "IdEmpleado", Name = "IX_MovimientoNomina_Nomina_Empleado")]
public partial class MovimientoNomina
{
    [Key]
    public int IdMovimiento { get; set; }

    public int IdNomina { get; set; }

    public int IdEmpleado { get; set; }

    public int IdRubro { get; set; }

    [StringLength(200)]
    public string Descripcion { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Monto { get; set; }

    [Required]
    [StringLength(50)]
    public string UsuarioIng { get; set; }

    public DateTime FechaIng { get; set; }

    [StringLength(50)]
    public string UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }

    [ForeignKey("IdEmpleado")]
    [InverseProperty("MovimientoNomina")]
    public virtual Empleado IdEmpleadoNavigation { get; set; }

    [ForeignKey("IdNomina")]
    [InverseProperty("MovimientoNomina")]
    public virtual Nomina IdNominaNavigation { get; set; }

    [ForeignKey("IdRubro")]
    [InverseProperty("MovimientoNomina")]
    public virtual Rubro IdRubroNavigation { get; set; }
}
