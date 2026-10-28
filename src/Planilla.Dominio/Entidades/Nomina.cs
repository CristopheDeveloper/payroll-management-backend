using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planilla.Dominio.Entidades;

[Index("IdTipoPlanilla", "Anio", "Mes", Name = "IX_Nomina_Tipo_Anio_Mes")]
[Index("Codigo", Name = "UQ_Nomina_Codigo", IsUnique = true)]
public partial class Nomina
{
    [Key]
    public int IdNomina { get; set; }

    public int IdTipoPlanilla { get; set; }

    [Required]
    [StringLength(20)]
    public string Codigo { get; set; }

    [Required]
    [StringLength(200)]
    public string Descripcion { get; set; }

    public int Anio { get; set; }

    public int Mes { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public DateOnly FechaPago { get; set; }

    [StringLength(500)]
    public string Comentario { get; set; }

    [Required]
    [StringLength(15)]
    public string Estado { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal TotalIngresos { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal TotalDescuentos { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal TotalLiquido { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal TotalPatronal { get; set; }

    public DateTime? FechaPreparacion { get; set; }

    public DateTime? FechaGeneracion { get; set; }

    [Column(TypeName = "decimal(9, 4)")]
    public decimal? IgssLaboral { get; set; }

    [Column(TypeName = "decimal(9, 4)")]
    public decimal? IgssPatronal { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? BonificacionIncentivo { get; set; }

    [Column(TypeName = "decimal(9, 4)")]
    public decimal? FactorHoraExtra { get; set; }

    public int? DiasMes { get; set; }

    public int? HorasMes { get; set; }

    [StringLength(50)]
    public string UsuarioCierre { get; set; }

    public DateTime? FechaCierre { get; set; }

    [StringLength(200)]
    public string MotivoReapertura { get; set; }

    [Required]
    public byte[] Version { get; set; }

    [Required]
    [StringLength(50)]
    public string UsuarioIng { get; set; }

    public DateTime FechaIng { get; set; }

    [StringLength(50)]
    public string UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }

    [ForeignKey("IdTipoPlanilla")]
    [InverseProperty("Nomina")]
    public virtual TipoPlanilla IdTipoPlanillaNavigation { get; set; }

    [InverseProperty("IdNominaNavigation")]
    public virtual ICollection<NominaEmpleado> NominaEmpleado { get; set; } = new List<NominaEmpleado>();
}
