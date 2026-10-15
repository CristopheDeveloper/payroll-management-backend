using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Planilla.Dominio.Entidades;

[Index("Fecha", Name = "IX_Bitacora_Fecha")]
[Index("Tabla", "IdRegistro", Name = "IX_Bitacora_Tabla_IdRegistro")]
public partial class Bitacora
{
    [Key]
    public long IdBitacora { get; set; }

    [Required]
    [StringLength(50)]
    public string Tabla { get; set; }

    public int IdRegistro { get; set; }

    [Required]
    [StringLength(10)]
    public string Accion { get; set; }

    public string ValoresAnteriores { get; set; }

    public string ValoresNuevos { get; set; }

    [Required]
    [StringLength(50)]
    public string Usuario { get; set; }

    public DateTime Fecha { get; set; }
}
