using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class NominaCrearDto
{
    [Range(2000, 2100, ErrorMessage = "El año debe estar entre 2000 y 2100.")]
    public int Anio { get; set; }

    [Range(1, 12, ErrorMessage = "El mes debe estar entre 1 y 12.")]
    public int Mes { get; set; }
}
