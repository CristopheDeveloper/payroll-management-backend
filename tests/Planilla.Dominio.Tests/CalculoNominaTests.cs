using Planilla.Dominio.Calculo;
using Planilla.Dominio.Constantes;
using Planilla.Dominio.Entidades;

namespace Planilla.Dominio.Tests;

public class CalculoNominaTests
{
    private static ParametrosNomina Parametros(int diasMes = 30)
    {
        return new ParametrosNomina
        {
            IgssLaboral = 4.83m,
            IgssPatronal = 10.67m,
            FactorHoraExtra = 1.5m,
            DiasMes = diasMes,
            HorasMes = 240
        };
    }

    private static PeriodoLaboral Periodo(DateOnly ingreso, DateOnly? baja = null)
    {
        return new PeriodoLaboral { FechaIngreso = ingreso, FechaBaja = baja };
    }

    private static DatosEmpleadoMes Datos(decimal salario, int diasPagar, decimal horasExtra = 0, decimal isr = 0,
        decimal bonificacion = 250m)
    {
        return new DatosEmpleadoMes
        {
            Salario = salario,
            Isr = isr,
            BonificacionIncentivo = bonificacion,
            DiasPagar = diasPagar,
            HorasExtra = horasExtra
        };
    }

    private static ValorManual Valor(int idRubro, string tipo, decimal monto, bool afectoIgss = false, int orden = 100)
    {
        return new ValorManual { IdRubro = idRubro, Tipo = tipo, Monto = monto, AfectoIgss = afectoIgss, Orden = orden };
    }

    private static List<RubroCalculado> Calcular(DatosEmpleadoMes datos, ParametrosNomina parametros = null)
    {
        return CalculoNomina.GenerarRubros(datos, parametros ?? Parametros());
    }

    private static decimal Monto(List<RubroCalculado> rubros, string codigo)
    {
        return rubros.Where(r => r.Codigo == codigo).Sum(r => r.Monto);
    }

    private static decimal Liquido(List<RubroCalculado> rubros)
    {
        return CalculoNomina.Total(rubros, "INGRESO") - CalculoNomina.Total(rubros, "DESCUENTO");
    }

    [Fact]
    public void EjemploJuanPerez()
    {
        var datos = Datos(4500m, 29, horasExtra: 2);
        datos.Valores.Add(Valor(9, "DESCUENTO", 500m, orden: 330));

        var rubros = Calcular(datos);

        Assert.Equal(4350.00m, Monto(rubros, CodigosRubro.SalarioOrdinario));
        Assert.Equal(56.25m, Monto(rubros, CodigosRubro.HorasExtra));
        Assert.Equal(241.67m, Monto(rubros, CodigosRubro.BonificacionIncentivo));
        Assert.Equal(4647.92m, CalculoNomina.Total(rubros, "INGRESO"));
        Assert.Equal(212.82m, Monto(rubros, CodigosRubro.IgssLaboral));
        Assert.Equal(712.82m, CalculoNomina.Total(rubros, "DESCUENTO"));
        Assert.Equal(3935.10m, Liquido(rubros));
        Assert.Equal(470.15m, Monto(rubros, CodigosRubro.IgssPatronal));
        Assert.Equal(470.15m, CalculoNomina.Total(rubros, "PATRONAL"));
    }

    [Fact]
    public void MultiplicaAntesDeDividir()
    {
        Assert.Equal(300.03m, Monto(Calcular(Datos(3000.25m, 3)), CodigosRubro.SalarioOrdinario));
    }

    [Fact]
    public void BaseIgssSinBonificacionNiIngresosNoAfectos()
    {
        var datos = Datos(4000m, 30);
        datos.Valores.Add(Valor(6, "INGRESO", 1000m, afectoIgss: false, orden: 150));
        datos.Valores.Add(Valor(5, "INGRESO", 500m, afectoIgss: true, orden: 140));

        var rubros = Calcular(datos);

        Assert.Equal(Math.Round(4500m * 4.83m / 100, 2, MidpointRounding.AwayFromZero), Monto(rubros, CodigosRubro.IgssLaboral));
        Assert.Equal(4500m, rubros.Single(r => r.Codigo == CodigosRubro.IgssLaboral).Base);
        Assert.Equal(4000m + 250m + 1000m + 500m, CalculoNomina.Total(rubros, "INGRESO"));
    }

    [Fact]
    public void IsrCeroSiNoHayBaseAfecta()
    {
        var rubros = Calcular(Datos(4000m, 0, isr: 150m));

        Assert.DoesNotContain(rubros, r => r.Codigo == CodigosRubro.Isr);
        Assert.Equal(0m, Liquido(rubros));
    }

    [Fact]
    public void IsrCompletoConPeriodoParcial()
    {
        Assert.Equal(150m, Monto(Calcular(Datos(4000m, 15, isr: 150m)), CodigosRubro.Isr));
    }

    [Fact]
    public void HorasExtraEnCeroNoGeneranRubro()
    {
        Assert.DoesNotContain(Calcular(Datos(4000m, 30)), r => r.Codigo == CodigosRubro.HorasExtra);
    }

    [Fact]
    public void ValoresManualesSalenEnOrdenYConSuDescripcion()
    {
        var datos = Datos(4000m, 30);
        datos.Valores.Add(Valor(20, "DESCUENTO", 100m, orden: 320));
        datos.Valores.Add(Valor(10, "DESCUENTO", 50m, orden: 300));
        datos.Valores.Add(Valor(30, "INGRESO", 200m, afectoIgss: true, orden: 100));
        datos.Valores[0].Descripcion = "Vidrio roto";

        var rubros = Calcular(datos);
        var manuales = rubros.Where(r => r.IdRubro != null).Select(r => r.IdRubro).ToList();

        Assert.Equal([30, 10, 20], manuales);
        Assert.Equal("Vidrio roto", rubros.Single(r => r.IdRubro == 20).Descripcion);
    }

    [Fact]
    public void ValorEnCeroSeIgnora()
    {
        var datos = Datos(4000m, 30);
        datos.Valores.Add(Valor(10, "DESCUENTO", 0m));

        Assert.DoesNotContain(Calcular(datos), r => r.IdRubro == 10);
    }

    [Fact]
    public void MesCompletoConCincoDiasDeSuspension()
    {
        var tramos = CalculoNomina.Tramos([Periodo(new DateOnly(2026, 1, 1))], 2026, 10);
        var diasPeriodo = CalculoNomina.DiasPeriodo(tramos, 30);
        var diasPagar = CalculoNomina.DiasPagar(diasPeriodo, 5, CalculoNomina.DiasCalendario(tramos));

        Assert.Equal(25, diasPagar);
        Assert.Equal(Math.Round(250m * 25 / 30, 2, MidpointRounding.AwayFromZero),
            Monto(Calcular(Datos(3000m, diasPagar)), CodigosRubro.BonificacionIncentivo));
    }

    [Fact]
    public void BonificacionPropiaDelEmpleado()
    {
        Assert.Equal(400m, Monto(Calcular(Datos(6000m, 24, bonificacion: 500m)), CodigosRubro.BonificacionIncentivo));
    }

    [Fact]
    public void BonificacionEnCeroNoGeneraRubro()
    {
        var rubros = Calcular(Datos(4000m, 30, bonificacion: 0m));

        Assert.DoesNotContain(rubros, r => r.Codigo == CodigosRubro.BonificacionIncentivo);
        Assert.Equal(4000m, CalculoNomina.Total(rubros, "INGRESO"));
    }

    [Fact]
    public void FaltasYSuspensionesSeSuman()
    {
        Assert.Equal(23, CalculoNomina.DiasPagar(30, 3 + 4, 31));
    }

    [Fact]
    public void SuspensionTodoElMesPagaCero()
    {
        Assert.Equal(0, CalculoNomina.DiasPagar(30, 31, 31));
    }

    [Fact]
    public void FebreroConVeintisieteFaltasPagaTres()
    {
        var tramos = CalculoNomina.Tramos([Periodo(new DateOnly(2025, 1, 1))], 2026, 2);
        var diasPeriodo = CalculoNomina.DiasPeriodo(tramos, 30);

        Assert.Equal(30, diasPeriodo);
        Assert.Equal(3, CalculoNomina.DiasPagar(diasPeriodo, 27, CalculoNomina.DiasCalendario(tramos)));
    }

    [Fact]
    public void FebreroConTodasLasFaltasPagaCero()
    {
        var tramos = CalculoNomina.Tramos([Periodo(new DateOnly(2025, 1, 1))], 2026, 2);

        Assert.Equal(0, CalculoNomina.DiasPagar(CalculoNomina.DiasPeriodo(tramos, 30), 28, CalculoNomina.DiasCalendario(tramos)));
    }

    [Fact]
    public void MesDeTreintaYUnDiasConTreintaFaltasPagaCero()
    {
        var tramos = CalculoNomina.Tramos([Periodo(new DateOnly(2025, 1, 1))], 2026, 10);

        Assert.Equal(0, CalculoNomina.DiasPagar(CalculoNomina.DiasPeriodo(tramos, 30), 30, CalculoNomina.DiasCalendario(tramos)));
    }

    [Fact]
    public void IngresoAMedioMes()
    {
        var tramos = CalculoNomina.Tramos([Periodo(new DateOnly(2026, 10, 16))], 2026, 10);

        Assert.Equal(15, CalculoNomina.DiasPeriodo(tramos, 30));
    }

    [Fact]
    public void IngresoElUltimoDiaDeFebreroCobraUnDia()
    {
        var tramos = CalculoNomina.Tramos([Periodo(new DateOnly(2026, 2, 28))], 2026, 2);

        Assert.Equal(1, CalculoNomina.DiasPeriodo(tramos, 30));
    }

    [Fact]
    public void ReingresoEnElMismoMes()
    {
        var periodos = new[]
        {
            Periodo(new DateOnly(2025, 1, 1), new DateOnly(2026, 10, 10)),
            Periodo(new DateOnly(2026, 10, 21))
        };

        var tramos = CalculoNomina.Tramos(periodos, 2026, 10);

        Assert.Equal(2, tramos.Count);
        Assert.Equal(10 + 10, CalculoNomina.DiasPeriodo(tramos, 30));
        Assert.Equal(10 + 11, CalculoNomina.DiasCalendario(tramos));
    }

    [Fact]
    public void PeriodosTraslapadosLanzanExcepcion()
    {
        var periodos = new[]
        {
            Periodo(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 20)),
            Periodo(new DateOnly(2026, 10, 15))
        };

        Assert.Throws<InvalidOperationException>(() => CalculoNomina.Tramos(periodos, 2026, 10));
    }

    [Fact]
    public void DiasMesTreintaYUnoPagaSalarioCompleto()
    {
        var tramos = CalculoNomina.Tramos([Periodo(new DateOnly(2025, 1, 1))], 2026, 10);
        var diasPeriodo = CalculoNomina.DiasPeriodo(tramos, 31);

        Assert.Equal(31, diasPeriodo);
        Assert.Equal(4000m, Monto(Calcular(Datos(4000m, diasPeriodo), Parametros(31)), CodigosRubro.SalarioOrdinario));
    }

    [Fact]
    public void LiquidoPuedeSerNegativoSiLosDescuentosSuperanLosIngresos()
    {
        var datos = Datos(4000m, 1);
        datos.Valores.Add(Valor(10, "DESCUENTO", 1000m));

        Assert.True(Liquido(Calcular(datos)) < 0);
    }
}
