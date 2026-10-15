using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planilla.Dominio.Entidades;

[Index("CodigoEmpleado", Name = "UQ_Empleado_CodigoEmpleado", IsUnique = true)]
[Index("Dpi", Name = "UQ_Empleado_Dpi", IsUnique = true)]
public partial class Empleado
{
    [Key]
    public int IdEmpleado { get; set; }

    [StringLength(15)]
    public string CodigoEmpleado { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombres { get; set; }

    [Required]
    [StringLength(100)]
    public string Apellidos { get; set; }

    [Required]
    [StringLength(13)]
    [Unicode(false)]
    public string Dpi { get; set; }

    [StringLength(20)]
    public string Pasaporte { get; set; }

    [StringLength(15)]
    public string Nit { get; set; }

    [StringLength(20)]
    public string NumeroIgss { get; set; }

    public DateOnly FechaNacimiento { get; set; }

    [Required]
    [StringLength(1)]
    [Unicode(false)]
    public string Genero { get; set; }

    [Required]
    [StringLength(200)]
    public string Direccion { get; set; }

    [StringLength(15)]
    public string Telefono { get; set; }

    [StringLength(100)]
    public string Correo { get; set; }

    [Required]
    [StringLength(15)]
    public string TipoPago { get; set; }

    [StringLength(50)]
    public string Banco { get; set; }

    [StringLength(30)]
    public string CuentaBancaria { get; set; }

    public int IdPuesto { get; set; }

    public int IdDepartamento { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Salario { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal RetencionIsr { get; set; }

    public bool Activo { get; set; }

    [Required]
    [StringLength(50)]
    public string UsuarioIng { get; set; }

    public DateTime FechaIng { get; set; }

    [StringLength(50)]
    public string UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }

    [ForeignKey("IdDepartamento")]
    [InverseProperty("Empleado")]
    public virtual Departamento IdDepartamentoNavigation { get; set; }

    [ForeignKey("IdPuesto")]
    [InverseProperty("Empleado")]
    public virtual Puesto IdPuestoNavigation { get; set; }

    [InverseProperty("IdEmpleadoNavigation")]
    public virtual ICollection<PeriodoLaboral> PeriodoLaboral { get; set; } = new List<PeriodoLaboral>();
}
