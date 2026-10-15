using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planilla.Dominio.Entidades;

[Index("Nombre", Name = "UQ_Puesto_Nombre", IsUnique = true)]
public partial class Puesto
{
    [Key]
    public int IdPuesto { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; }

    [StringLength(200)]
    public string Descripcion { get; set; }

    public bool Activo { get; set; }

    [Required]
    [StringLength(50)]
    public string UsuarioIng { get; set; }

    public DateTime FechaIng { get; set; }

    [StringLength(50)]
    public string UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }

    [InverseProperty("IdPuestoNavigation")]
    public virtual ICollection<Empleado> Empleado { get; set; } = new List<Empleado>();
}
