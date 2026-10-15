using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planilla.Dominio.Entidades;

[Index("IdNomina", "IdEmpleado", Name = "UQ_NominaDetalle_Nomina_Empleado", IsUnique = true)]
public partial class NominaDetalle
{
    [Key]
    public int IdNominaDetalle { get; set; }

    public int IdNomina { get; set; }

    public int IdEmpleado { get; set; }

    public int IdPuesto { get; set; }

    public int IdDepartamento { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Salario { get; set; }

    [Required]
    [StringLength(15)]
    public string TipoPago { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal RetencionIsr { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Isr { get; set; }

    public int DiasPeriodo { get; set; }

    public int DiasFalta { get; set; }

    public int DiasSuspension { get; set; }

    public int DiasPagar { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal HorasExtra { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal SalarioDevengado { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal MontoHorasExtra { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Bonificacion { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal OtrosIngresosAfectos { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal OtrosIngresosNoAfectos { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal TotalIngresos { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal IgssLaboral { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal OtrosDescuentos { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal TotalDescuentos { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal LiquidoRecibir { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal IgssPatronal { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Irtra { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Intecap { get; set; }

    [Required]
    [StringLength(50)]
    public string UsuarioIng { get; set; }

    public DateTime FechaIng { get; set; }

    [StringLength(50)]
    public string UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }

    [ForeignKey("IdDepartamento")]
    [InverseProperty("NominaDetalle")]
    public virtual Departamento IdDepartamentoNavigation { get; set; }

    [ForeignKey("IdEmpleado")]
    [InverseProperty("NominaDetalle")]
    public virtual Empleado IdEmpleadoNavigation { get; set; }

    [ForeignKey("IdNomina")]
    [InverseProperty("NominaDetalle")]
    public virtual Nomina IdNominaNavigation { get; set; }

    [ForeignKey("IdPuesto")]
    [InverseProperty("NominaDetalle")]
    public virtual Puesto IdPuestoNavigation { get; set; }
}
