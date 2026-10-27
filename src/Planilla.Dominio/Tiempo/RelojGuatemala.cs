namespace Planilla.Dominio.Tiempo;

/// <summary>
/// Proporciona la fecha y hora civil de Guatemala sin depender de la zona horaria del servidor.
/// </summary>
public static class RelojGuatemala
{
    private static readonly TimeZoneInfo ZonaHoraria =
        TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala");

    public static DateTime Ahora => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaHoraria);

    public static DateOnly Hoy => DateOnly.FromDateTime(Ahora);
}
