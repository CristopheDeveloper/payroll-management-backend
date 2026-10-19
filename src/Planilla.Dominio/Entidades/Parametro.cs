using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planilla.Dominio.Entidades;

[Index("Clave", Name = "UQ_Parametro_Clave", IsUnique = true)]
public partial class Parametro
{
    [Key]
    public int IdParametro { get; set; }

    [Required]
    [StringLength(50)]
    public string Clave { get; set; }

    [Required]
    [StringLength(100)]
    public string Valor { get; set; }

    [Required]
    [StringLength(10)]
    public string TipoDato { get; set; }

    [Required]
    [StringLength(200)]
    public string Descripcion { get; set; }

    [Required]
    [StringLength(50)]
    public string UsuarioIng { get; set; }

    public DateTime FechaIng { get; set; }

    [StringLength(50)]
    public string UsuarioAct { get; set; }

    public DateTime? FechaAct { get; set; }
}
