using Planilla.Dominio.Calculo;
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
            Irtra = 1m,
            Intecap = 1m,
            BonificacionIncentivo = 250m,
            FactorHoraExtra = 1.5m,
            DiasMes = diasMes,
            HorasMes = 240
        };
    }

    private static PeriodoLaboral Periodo(DateOnly ingreso, DateOnly? baja = null)
    {
        return new PeriodoLaboral { FechaIngreso = ingreso, FechaBaja = baja };
    }

    private static DatosEmpleadoMes Datos(decimal salario, int diasPagar, decimal horasExtra = 0, decimal retencionIsr = 0,
        decimal otrosDescuentos = 0, int diasPeriodo = 30)
    {
        return new DatosEmpleadoMes
        {
            Salario = salario,
            RetencionIsr = retencionIsr,
            IdPuesto = 1,
            IdDepartamento = 1,
            TipoPago = "TRANSFERENCIA",
            DiasPeriodo = diasPeriodo,
            DiasPagar = diasPagar,
            HorasExtra = horasExtra,
            OtrosDescuentos = otrosDescuentos
        };
    }

    private static NominaDetalle Calcular(DatosEmpleadoMes datos, ParametrosNomina parametros = null)
    {
        var fila = new NominaDetalle();
        CalculoNomina.ActualizarFila(fila, datos, new HashSet<string>(), true, parametros ?? Parametros());
        return fila;
    }

    [Fact]
    public void EjemploJuanPerez()
    {
        var fila = Calcular(Datos(4500m, 29, horasExtra: 2, otrosDescuentos: 500m));

        Assert.Equal(4350.00m, fila.SalarioDevengado);
        Assert.Equal(56.25m, fila.MontoHorasExtra);
        Assert.Equal(241.67m, fila.Bonificacion);
        Assert.Equal(4647.92m, fila.TotalIngresos);
        Assert.Equal(212.82m, fila.IgssLaboral);
        Assert.Equal(712.82m, fila.TotalDescuentos);
        Assert.Equal(3935.10m, fila.LiquidoRecibir);
        Assert.Equal(470.15m, fila.IgssPatronal);
        Assert.Equal(44.06m, fila.Irtra);
        Assert.Equal(44.06m, fila.Intecap);
    }

    [Fact]
    public void MultiplicaAntesDeDividir()
    {
        var fila = Calcular(Datos(3000.25m, 3));

        Assert.Equal(300.03m, fila.SalarioDevengado);
    }

    [Fact]
    public void BaseIgssSinBonificacionNiIngresosNoAfectos()
    {
        var datos = Datos(4000m, 30);
        datos.OtrosIngresosNoAfectos = 1000m;
        datos.OtrosIngresosAfectos = 500m;

        var fila = Calcular(datos);

        Assert.Equal(Math.Round(4500m * 4.83m / 100, 2, MidpointRounding.AwayFromZero), fila.IgssLaboral);
        Assert.Equal(4000m + 250m + 1000m + 500m, fila.TotalIngresos);
    }

    [Fact]
    public void IsrCeroSiNoHayBaseAfecta()
    {
        var fila = Calcular(Datos(4000m, 0, retencionIsr: 150m));

        Assert.Equal(150m, fila.RetencionIsr);
        Assert.Equal(0m, fila.Isr);
        Assert.Equal(0m, fila.LiquidoRecibir);
    }

    [Fact]
    public void IsrCompletoConPeriodoParcial()
    {
        var fila = Calcular(Datos(4000m, 15, retencionIsr: 150m));

        Assert.Equal(150m, fila.Isr);
    }

    [Fact]
    public void IsrVuelveAlRecalcularDesdeLaRetencionGuardada()
    {
        var fila = Calcular(Datos(4000m, 0, retencionIsr: 150m));
        Assert.Equal(0m, fila.Isr);

        CalculoNomina.ActualizarFila(fila, Datos(5000m, 30, retencionIsr: 999m), new HashSet<string>(), false, Parametros());

        Assert.Equal(4000m, fila.Salario);
        Assert.Equal(150m, fila.RetencionIsr);
        Assert.Equal(150m, fila.Isr);
    }

    [Fact]
    public void ActualizarFilaConValorActual()
    {
        var fila = Calcular(Datos(4000m, 30));

        CalculoNomina.ActualizarFila(fila, Datos(5000m, 30), new HashSet<string> { CalculoNomina.CampoSalario }, false, Parametros());

        Assert.Equal(5000m, fila.Salario);
        Assert.Equal(5000m, fila.SalarioDevengado);
    }

    [Fact]
    public void MesCompletoConCincoDiasDeSuspension()
    {
        var tramos = CalculoNomina.Tramos([Periodo(new DateOnly(2026, 1, 1))], 2026, 10);
        var diasPeriodo = CalculoNomina.DiasPeriodo(tramos, 30);
        var diasPagar = CalculoNomina.DiasPagar(diasPeriodo, 5, CalculoNomina.DiasCalendario(tramos));

        Assert.Equal(25, diasPagar);
        Assert.Equal(Math.Round(250m * 25 / 30, 2, MidpointRounding.AwayFromZero), Calcular(Datos(3000m, diasPagar)).Bonificacion);
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
        Assert.Equal(4000m, Calcular(Datos(4000m, diasPeriodo, diasPeriodo: diasPeriodo), Parametros(31)).SalarioDevengado);
    }

    [Fact]
    public void FirmaNoCambiaSiSeRecalculaIgual()
    {
        var fila = Calcular(Datos(4500m, 29, horasExtra: 2, otrosDescuentos: 500m));
        var antes = CalculoNomina.Firma(fila);

        CalculoNomina.ActualizarFila(fila, Datos(9999m, 29, horasExtra: 2, otrosDescuentos: 500m), new HashSet<string>(), false, Parametros());

        Assert.Equal(antes, CalculoNomina.Firma(fila));
    }

    [Fact]
    public void FirmaCambiaSiCambianLasFaltas()
    {
        var fila = Calcular(Datos(4500m, 30));
        var antes = CalculoNomina.Firma(fila);

        CalculoNomina.ActualizarFila(fila, Datos(4500m, 29), new HashSet<string>(), false, Parametros());

        Assert.NotEqual(antes, CalculoNomina.Firma(fila));
    }
}
