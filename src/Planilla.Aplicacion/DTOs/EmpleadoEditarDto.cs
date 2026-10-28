using System.ComponentModel.DataAnnotations;

namespace Planilla.Aplicacion.DTOs;

public class EmpleadoEditarDto
{
    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los nombres deben tener como máximo 100 caracteres.")]
    public string Nombres { get; set; }

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los apellidos deben tener como máximo 100 caracteres.")]
    public string Apellidos { get; set; }

    [Required(ErrorMessage = "El DPI es obligatorio.")]
    [RegularExpression(@"^\s*[0-9]{13}\s*$", ErrorMessage = "El DPI debe tener 13 dígitos, sin espacios ni guiones.")]
    public string Dpi { get; set; }

    [StringLength(20, ErrorMessage = "El pasaporte debe tener como máximo 20 caracteres.")]
    public string Pasaporte { get; set; }

    [StringLength(15, ErrorMessage = "El NIT debe tener como máximo 15 caracteres.")]
    public string Nit { get; set; }

    [StringLength(20, ErrorMessage = "El número de IGSS debe tener como máximo 20 caracteres.")]
    public string NumeroIgss { get; set; }

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public DateOnly? FechaNacimiento { get; set; }

    [Required(ErrorMessage = "El género es obligatorio.")]
    [AllowedValues("M", "F", ErrorMessage = "El género debe ser M o F.")]
    public string Genero { get; set; }

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(200, ErrorMessage = "La dirección debe tener como máximo 200 caracteres.")]
    public string Direccion { get; set; }

    [StringLength(15, ErrorMessage = "El teléfono debe tener como máximo 15 caracteres.")]
    public string Telefono { get; set; }

    [StringLength(100, ErrorMessage = "El correo debe tener como máximo 100 caracteres.")]
    public string Correo { get; set; }

    [Required(ErrorMessage = "La forma de pago es obligatoria.")]
    [AllowedValues("EFECTIVO", "CHEQUE", "TRANSFERENCIA", ErrorMessage = "La forma de pago debe ser EFECTIVO, CHEQUE o TRANSFERENCIA.")]
    public string TipoPago { get; set; }

    [StringLength(50, ErrorMessage = "El banco debe tener como máximo 50 caracteres.")]
    public string Banco { get; set; }

    [StringLength(30, ErrorMessage = "La cuenta bancaria debe tener como máximo 30 caracteres.")]
    public string CuentaBancaria { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El puesto es obligatorio.")]
    public int IdPuesto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El departamento es obligatorio.")]
    public int IdDepartamento { get; set; }

    [Range(typeof(decimal), "0.01", "999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El salario debe estar entre 0.01 y 999,999.99.")]
    public decimal Salario { get; set; }

    [Range(typeof(decimal), "0", "999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "La retención de ISR debe estar entre 0 y 999,999.99.")]
    public decimal RetencionIsr { get; set; }

    [Required(ErrorMessage = "La bonificación incentivo es obligatoria.")]
    [Range(typeof(decimal), "0", "999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "La bonificación incentivo debe estar entre 0 y 999,999.99.")]
    public decimal? BonificacionIncentivo { get; set; }
}
