using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planilla.Dominio.Entidades;

public partial class PeriodoLaboral
{
    [Key]
    public int IdPeriodoLaboral { get; set; }

    public int IdEmpleado { get; set; }

    public DateOnly FechaIngreso { get; set; }

    public DateOnly? FechaBaja { get; set; }

    [StringLength(20)]
    public string TipoBaja { get; set; }

    [StringLength(200)]
    public string MotivoBaja { get; set; }

    [Required]
    [StringLength(50)]
    public string UsuarioIng { get; set; }

    public DateTime FechaIng { get; set; }

    [StringLength(50)]
    public string UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }

    [ForeignKey("IdEmpleado")]
    [InverseProperty("PeriodoLaboral")]
    public virtual Empleado IdEmpleadoNavigation { get; set; }
}
