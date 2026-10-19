using System.Globalization;
using Planilla.Dominio.Entidades;

namespace Planilla.Dominio.Calculo;

public static class CalculoNomina
{
    public const string CampoSalario = "Salario";
    public const string CampoRetencionIsr = "RetencionIsr";
    public const string CampoIdPuesto = "IdPuesto";
    public const string CampoIdDepartamento = "IdDepartamento";
    public const string CampoTipoPago = "TipoPago";

    public static decimal Redondear(decimal valor)
    {
        return Math.Round(valor, 2, MidpointRounding.AwayFromZero);
    }

    public static int DiaComercial(DateOnly fecha, int diasMes)
    {
        return fecha.Day == DateTime.DaysInMonth(fecha.Year, fecha.Month) ? diasMes : Math.Min(fecha.Day, diasMes);
    }

    public static List<(DateOnly Desde, DateOnly Hasta)> Tramos(IEnumerable<PeriodoLaboral> periodos, int anio, int mes)
    {
        var inicio = new DateOnly(anio, mes, 1);
        var fin = inicio.AddMonths(1).AddDays(-1);

        var tramos = periodos
            .Where(p => p.FechaIngreso <= fin && (p.FechaBaja == null || p.FechaBaja.Value >= inicio))
            .Select(p => (
                Desde: p.FechaIngreso > inicio ? p.FechaIngreso : inicio,
                Hasta: p.FechaBaja == null || p.FechaBaja.Value > fin ? fin : p.FechaBaja.Value))
            .OrderBy(t => t.Desde)
            .ToList();

        for (var i = 1; i < tramos.Count; i++)
        {
            if (tramos[i].Desde <= tramos[i - 1].Hasta)
            {
                throw new InvalidOperationException($"Los periodos laborales se traslapan en {mes:00}/{anio}.");
            }
        }

        return tramos;
    }

    public static int DiasPeriodo(List<(DateOnly Desde, DateOnly Hasta)> tramos, int diasMes)
    {
        var dias = tramos.Sum(t => DiaComercial(t.Hasta, diasMes) - DiaComercial(t.Desde, diasMes) + 1);
        return Math.Min(dias, diasMes);
    }

    public static int DiasCalendario(List<(DateOnly Desde, DateOnly Hasta)> tramos)
    {
        return tramos.Sum(t => t.Hasta.DayNumber - t.Desde.DayNumber + 1);
    }

    public static int DiasPagar(int diasPeriodo, int diasNoPagados, int diasCalendario)
    {
        return diasNoPagados >= diasCalendario ? 0 : Math.Max(0, diasPeriodo - diasNoPagados);
    }

    public static decimal IsrAplicable(decimal retencionIsr, decimal baseIgss)
    {
        return baseIgss == 0 ? 0 : retencionIsr;
    }

    public static void ActualizarFila(NominaDetalle fila, DatosEmpleadoMes datos, ISet<string> camposConValorActual,
        bool esNueva, ParametrosNomina parametros)
    {
        if (esNueva || camposConValorActual.Contains(CampoSalario))
        {
            fila.Salario = datos.Salario;
        }
        if (esNueva || camposConValorActual.Contains(CampoRetencionIsr))
        {
            fila.RetencionIsr = datos.RetencionIsr;
        }
        if (esNueva || camposConValorActual.Contains(CampoIdPuesto))
        {
            fila.IdPuesto = datos.IdPuesto;
        }
        if (esNueva || camposConValorActual.Contains(CampoIdDepartamento))
        {
            fila.IdDepartamento = datos.IdDepartamento;
        }
        if (esNueva || camposConValorActual.Contains(CampoTipoPago))
        {
            fila.TipoPago = datos.TipoPago;
        }

        fila.DiasPeriodo = datos.DiasPeriodo;
        fila.DiasFalta = datos.DiasFalta;
        fila.DiasSuspension = datos.DiasSuspension;
        fila.DiasPagar = datos.DiasPagar;
        fila.HorasExtra = datos.HorasExtra;
        fila.OtrosIngresosAfectos = datos.OtrosIngresosAfectos;
        fila.OtrosIngresosNoAfectos = datos.OtrosIngresosNoAfectos;
        fila.OtrosDescuentos = datos.OtrosDescuentos;

        CalcularMontos(fila, parametros);
    }

    public static void CalcularMontos(NominaDetalle fila, ParametrosNomina p)
    {
        fila.SalarioDevengado = Redondear(fila.Salario * fila.DiasPagar / p.DiasMes);
        fila.MontoHorasExtra = Redondear(fila.Salario * p.FactorHoraExtra * fila.HorasExtra / p.HorasMes);
        fila.Bonificacion = Redondear(p.BonificacionIncentivo * fila.DiasPagar / p.DiasMes);
        fila.TotalIngresos = fila.SalarioDevengado + fila.MontoHorasExtra + fila.Bonificacion
            + fila.OtrosIngresosAfectos + fila.OtrosIngresosNoAfectos;

        var baseIgss = fila.SalarioDevengado + fila.MontoHorasExtra + fila.OtrosIngresosAfectos;
        fila.IgssLaboral = Redondear(baseIgss * p.IgssLaboral / 100);
        fila.Isr = IsrAplicable(fila.RetencionIsr, baseIgss);
        fila.TotalDescuentos = fila.IgssLaboral + fila.Isr + fila.OtrosDescuentos;
        fila.LiquidoRecibir = fila.TotalIngresos - fila.TotalDescuentos;

        fila.IgssPatronal = Redondear(baseIgss * p.IgssPatronal / 100);
        fila.Irtra = Redondear(baseIgss * p.Irtra / 100);
        fila.Intecap = Redondear(baseIgss * p.Intecap / 100);
    }

    public static string Firma(NominaDetalle fila)
    {
        var montos = new[]
        {
            fila.Salario, fila.RetencionIsr, fila.Isr, fila.HorasExtra, fila.SalarioDevengado, fila.MontoHorasExtra,
            fila.Bonificacion, fila.OtrosIngresosAfectos, fila.OtrosIngresosNoAfectos, fila.TotalIngresos,
            fila.IgssLaboral, fila.OtrosDescuentos, fila.TotalDescuentos, fila.LiquidoRecibir, fila.IgssPatronal,
            fila.Irtra, fila.Intecap
        };
        var enteros = new[]
        {
            fila.IdPuesto, fila.IdDepartamento, fila.DiasPeriodo, fila.DiasFalta, fila.DiasSuspension, fila.DiasPagar
        };

        return string.Join("|", montos.Select(m => m.ToString("0.00", CultureInfo.InvariantCulture)))
            + "|" + string.Join("|", enteros)
            + "|" + fila.TipoPago;
    }
}
