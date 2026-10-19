using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planilla.Dominio.Entidades;

[Index("Fecha", Name = "IX_Asistencia_Fecha")]
[Index("IdEmpleado", "Fecha", Name = "UQ_Asistencia_Empleado_Fecha", IsUnique = true)]
public partial class Asistencia
{
    [Key]
    public int IdAsistencia { get; set; }

    public int IdEmpleado { get; set; }

    public DateOnly Fecha { get; set; }

    [Required]
    [StringLength(20)]
    public string Estado { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal HorasExtra { get; set; }

    [StringLength(200)]
    public string Observacion { get; set; }

    [Required]
    [StringLength(50)]
    public string UsuarioIng { get; set; }

    public DateTime FechaIng { get; set; }

    [StringLength(50)]
    public string UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }

    [ForeignKey("IdEmpleado")]
    [InverseProperty("Asistencia")]
    public virtual Empleado IdEmpleadoNavigation { get; set; }
}
