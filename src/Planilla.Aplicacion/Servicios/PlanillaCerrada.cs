using Planilla.Aplicacion.Excepciones;
using Planilla.Aplicacion.Interfaces;

namespace Planilla.Aplicacion.Servicios;

public static class PlanillaCerrada
{
    public static async Task Validar(INominaRepositorio nominaRepositorio, DateOnly desde, DateOnly hasta)
    {
        var cerradas = await nominaRepositorio.ObtenerCerradasEntre(desde, hasta);
        if (cerradas.Count == 0)
        {
            return;
        }

        var meses = string.Join(", ", cerradas.Select(n => $"{n.Mes:00}/{n.Anio}"));
        throw new ReglaNegocioException(cerradas.Count == 1
            ? $"La planilla de {meses} está cerrada; pida al administrador que la reabra."
            : $"Las planillas de {meses} están cerradas; pida al administrador que las reabra.", 409);
    }
}
