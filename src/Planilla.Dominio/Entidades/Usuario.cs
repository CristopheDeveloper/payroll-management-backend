using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planilla.Dominio.Entidades;

[Index("NombreUsuario", Name = "UQ_Usuario_NombreUsuario", IsUnique = true)]
public partial class Usuario
{
    [Key]
    public int IdUsuario { get; set; }

    [Required]
    [StringLength(50)]
    public string NombreUsuario { get; set; }

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; }

    [Required]
    [StringLength(150)]
    public string NombreCompleto { get; set; }

    public int IdRol { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public bool Activo { get; set; }

    [Required]
    [StringLength(50)]
    public string UsuarioIng { get; set; }

    public DateTime FechaIng { get; set; }

    [StringLength(50)]
    public string UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }

    [ForeignKey("IdRol")]
    [InverseProperty("Usuario")]
    public virtual Rol IdRolNavigation { get; set; }
}
