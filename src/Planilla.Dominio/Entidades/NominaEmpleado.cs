using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planilla.Dominio.Entidades;

[Index("IdNomina", "IdEmpleado", Name = "UQ_NominaEmpleado_Nomina_Empleado", IsUnique = true)]
public partial class NominaEmpleado
{
    [Key]
    public int IdNominaEmpleado { get; set; }

    public int IdNomina { get; set; }

    public int IdEmpleado { get; set; }

    public int IdPuesto { get; set; }

    public int IdDepartamento { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Salario { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal RetencionIsr { get; set; }

    [Required]
    [StringLength(15)]
    public string TipoPago { get; set; }

    [StringLength(50)]
    public string Banco { get; set; }

    [StringLength(30)]
    public string CuentaBancaria { get; set; }

    public int DiasPeriodo { get; set; }

    public int DiasFalta { get; set; }

    public int DiasSuspension { get; set; }

    public int DiasPagarSugeridos { get; set; }

    public int DiasPagar { get; set; }

    [Column(TypeName = "decimal(6, 2)")]
    public decimal HorasExtraSugeridas { get; set; }

    [Column(TypeName = "decimal(6, 2)")]
    public decimal HorasExtra { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Isr { get; set; }

    public bool DatosAjustados { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal TotalIngresos { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal TotalDescuentos { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal Liquido { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal TotalPatronal { get; set; }

    [Required]
    [StringLength(50)]
    public string UsuarioIng { get; set; }

    public DateTime FechaIng { get; set; }

    [StringLength(50)]
    public string UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }

    [ForeignKey("IdDepartamento")]
    [InverseProperty("NominaEmpleado")]
    public virtual Departamento IdDepartamentoNavigation { get; set; }

    [ForeignKey("IdEmpleado")]
    [InverseProperty("NominaEmpleado")]
    public virtual Empleado IdEmpleadoNavigation { get; set; }

    [ForeignKey("IdNomina")]
    [InverseProperty("NominaEmpleado")]
    public virtual Nomina IdNominaNavigation { get; set; }

    [ForeignKey("IdPuesto")]
    [InverseProperty("NominaEmpleado")]
    public virtual Puesto IdPuestoNavigation { get; set; }

    [InverseProperty("IdNominaEmpleadoNavigation")]
    public virtual ICollection<NominaRubro> NominaRubro { get; set; } = new List<NominaRubro>();

    [InverseProperty("IdNominaEmpleadoNavigation")]
    public virtual ICollection<NominaValor> NominaValor { get; set; } = new List<NominaValor>();
}
