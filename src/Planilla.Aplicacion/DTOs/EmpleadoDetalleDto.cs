namespace Planilla.Aplicacion.DTOs;

public class EmpleadoDetalleDto
{
    public int IdEmpleado { get; set; }
    public string CodigoEmpleado { get; set; }
    public string Nombres { get; set; }
    public string Apellidos { get; set; }
    public string Dpi { get; set; }
    public string Pasaporte { get; set; }
    public string Nit { get; set; }
    public string NumeroIgss { get; set; }
    public DateOnly FechaNacimiento { get; set; }
    public string Genero { get; set; }
    public string Direccion { get; set; }
    public string Telefono { get; set; }
    public string Correo { get; set; }
    public string TipoPago { get; set; }
    public string Banco { get; set; }
    public string CuentaBancaria { get; set; }
    public int IdPuesto { get; set; }
    public string NombrePuesto { get; set; }
    public int IdDepartamento { get; set; }
    public string NombreDepartamento { get; set; }
    public decimal Salario { get; set; }
    public decimal RetencionIsr { get; set; }
    public decimal BonificacionIncentivo { get; set; }
    public bool Activo { get; set; }
    public List<PeriodoLaboralDto> Periodos { get; set; }
}
