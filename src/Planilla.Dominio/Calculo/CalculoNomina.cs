using Planilla.Dominio.Constantes;
using Planilla.Dominio.Entidades;

namespace Planilla.Dominio.Calculo;

public static class CalculoNomina
{
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

    public static List<RubroCalculado> GenerarRubros(DatosEmpleadoMes datos, ParametrosNomina p)
    {
        var rubros = new List<RubroCalculado>();

        var salario = Redondear(datos.Salario * datos.DiasPagar / p.DiasMes);
        rubros.Add(Sistema(CodigosRubro.SalarioOrdinario, "INGRESO", null, salario));

        var horasExtra = Redondear(datos.Salario * p.FactorHoraExtra * datos.HorasExtra / p.HorasMes);
        if (datos.HorasExtra > 0)
        {
            rubros.Add(Sistema(CodigosRubro.HorasExtra, "INGRESO", null, horasExtra));
        }

        if (datos.BonificacionIncentivo > 0)
        {
            rubros.Add(Sistema(CodigosRubro.BonificacionIncentivo, "INGRESO", null,
                Redondear(datos.BonificacionIncentivo * datos.DiasPagar / p.DiasMes)));
        }

        var valores = datos.Valores.Where(v => v.Monto > 0).OrderBy(v => v.Orden).ToList();
        foreach (var valor in valores.Where(v => v.Tipo == "INGRESO"))
        {
            rubros.Add(Manual(valor));
        }

        var baseIgss = salario + horasExtra + valores.Where(v => v.Tipo == "INGRESO" && v.AfectoIgss).Sum(v => v.Monto);
        rubros.Add(Sistema(CodigosRubro.IgssLaboral, "DESCUENTO", baseIgss, Redondear(baseIgss * p.IgssLaboral / 100)));

        var isr = IsrAplicable(datos.Isr, baseIgss);
        if (isr > 0)
        {
            rubros.Add(Sistema(CodigosRubro.Isr, "DESCUENTO", null, isr));
        }

        foreach (var valor in valores.Where(v => v.Tipo == "DESCUENTO"))
        {
            rubros.Add(Manual(valor));
        }

        rubros.Add(Sistema(CodigosRubro.IgssPatronal, "PATRONAL", baseIgss, Redondear(baseIgss * p.IgssPatronal / 100)));

        return rubros;
    }

    public static decimal Total(IEnumerable<RubroCalculado> rubros, string tipo)
    {
        return rubros.Where(r => r.Tipo == tipo).Sum(r => r.Monto);
    }

    private static RubroCalculado Sistema(string codigo, string tipo, decimal? baseCalculo, decimal monto)
    {
        return new RubroCalculado { Codigo = codigo, Tipo = tipo, Base = baseCalculo, Monto = monto };
    }

    private static RubroCalculado Manual(ValorManual valor)
    {
        return new RubroCalculado
        {
            IdRubro = valor.IdRubro,
            Tipo = valor.Tipo,
            Monto = valor.Monto,
            Descripcion = valor.Descripcion
        };
    }
}
