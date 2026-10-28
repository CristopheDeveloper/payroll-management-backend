using System.Globalization;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Excepciones;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Constantes;
using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Servicios;

public class ReporteServicio : IReporteServicio
{
    private readonly INominaRepositorio _nominaRepositorio;
    private readonly IParametroRepositorio _parametroRepositorio;

    public ReporteServicio(INominaRepositorio nominaRepositorio, IParametroRepositorio parametroRepositorio)
    {
        _nominaRepositorio = nominaRepositorio;
        _parametroRepositorio = parametroRepositorio;
    }

    public async Task<ReportePlanillaDto> ObtenerPlanilla(int idNomina)
    {
        var nomina = await ObtenerGenerada(idNomina);
        var empleados = await _nominaRepositorio.ObtenerEmpleadosNomina(idNomina);

        var departamentos = empleados
            .GroupBy(f => f.IdDepartamento)
            .Select(g => new DepartamentoReporteDto
            {
                NombreDepartamento = g.First().IdDepartamentoNavigation.Nombre,
                Filas = g.Select(AFila).ToList()
            })
            .OrderBy(d => d.NombreDepartamento)
            .ToList();

        foreach (var departamento in departamentos)
        {
            departamento.Subtotal = Sumar(departamento.Filas);
        }

        var cuota = new CuotaPatronalDto
        {
            IgssPatronal = empleados.Sum(f => Monto(f, CodigosRubro.IgssPatronal))
        };
        cuota.Total = cuota.IgssPatronal;

        return new ReportePlanillaDto
        {
            Encabezado = await CrearEncabezado(nomina),
            Departamentos = departamentos,
            Total = Sumar(departamentos.SelectMany(d => d.Filas).ToList()),
            CuotaPatronal = cuota
        };
    }

    public async Task<ReporteBoletasDto> ObtenerBoletas(int idNomina, int? idEmpleado)
    {
        var nomina = await ObtenerGenerada(idNomina);
        var empleados = (await _nominaRepositorio.ObtenerEmpleadosNomina(idNomina))
            .Where(f => idEmpleado == null || f.IdEmpleado == idEmpleado)
            .ToList();
        if (idEmpleado != null && empleados.Count == 0)
        {
            throw new ReglaNegocioException("El empleado no está en la planilla.", 404);
        }

        return new ReporteBoletasDto
        {
            Encabezado = await CrearEncabezado(nomina),
            Boletas = empleados.Select(ABoleta).ToList()
        };
    }

    private async Task<Nomina> ObtenerGenerada(int idNomina)
    {
        var nomina = await _nominaRepositorio.ObtenerPorId(idNomina)
            ?? throw new ReglaNegocioException("La planilla no existe.", 404);
        if (nomina.Estado != "GENERADA" && nomina.Estado != "CERRADA")
        {
            throw new ReglaNegocioException("Genere la planilla antes de imprimir reportes.", 409);
        }
        return nomina;
    }

    private async Task<EncabezadoReporteDto> CrearEncabezado(Nomina nomina)
    {
        return new EncabezadoReporteDto
        {
            IdNomina = nomina.IdNomina,
            Codigo = nomina.Codigo,
            Descripcion = nomina.Descripcion,
            NombreEmpresa = (await _parametroRepositorio.ObtenerPorClave(ClavesParametro.NombreEmpresa))?.Valor,
            NitEmpresa = (await _parametroRepositorio.ObtenerPorClave(ClavesParametro.NitEmpresa))?.Valor,
            DireccionEmpresa = (await _parametroRepositorio.ObtenerPorClave(ClavesParametro.DireccionEmpresa))?.Valor,
            Anio = nomina.Anio,
            Mes = nomina.Mes,
            FechaInicio = nomina.FechaInicio,
            FechaFin = nomina.FechaFin,
            Estado = nomina.Estado,
            Preliminar = nomina.Estado != "CERRADA",
            FechaCierre = nomina.Estado == "CERRADA" ? nomina.FechaCierre : null,
            UsuarioCierre = nomina.Estado == "CERRADA" ? nomina.UsuarioCierre : null
        };
    }

    private static FilaPlanillaDto AFila(NominaEmpleado f)
    {
        var salario = Monto(f, CodigosRubro.SalarioOrdinario);
        var horasExtra = Monto(f, CodigosRubro.HorasExtra);
        var bonificacion = Monto(f, CodigosRubro.BonificacionIncentivo);
        var igss = Monto(f, CodigosRubro.IgssLaboral);
        var isr = Monto(f, CodigosRubro.Isr);

        return new FilaPlanillaDto
        {
            IdEmpleado = f.IdEmpleado,
            CodigoEmpleado = f.IdEmpleadoNavigation.CodigoEmpleado,
            NombreCompleto = NombreCompleto(f.IdEmpleadoNavigation),
            NombrePuesto = f.IdPuestoNavigation.Nombre,
            DiasPagar = f.DiasPagar,
            HorasExtra = f.HorasExtra,
            SalarioDevengado = salario,
            MontoHorasExtra = horasExtra,
            Bonificacion = bonificacion,
            OtrosIngresos = f.TotalIngresos - salario - horasExtra - bonificacion,
            TotalIngresos = f.TotalIngresos,
            IgssLaboral = igss,
            Isr = isr,
            OtrosDescuentos = f.TotalDescuentos - igss - isr,
            TotalDescuentos = f.TotalDescuentos,
            LiquidoRecibir = f.Liquido
        };
    }

    private static TotalesReporteDto Sumar(List<FilaPlanillaDto> filas)
    {
        return new TotalesReporteDto
        {
            CantidadEmpleados = filas.Count,
            SalarioDevengado = filas.Sum(f => f.SalarioDevengado),
            MontoHorasExtra = filas.Sum(f => f.MontoHorasExtra),
            Bonificacion = filas.Sum(f => f.Bonificacion),
            OtrosIngresos = filas.Sum(f => f.OtrosIngresos),
            TotalIngresos = filas.Sum(f => f.TotalIngresos),
            IgssLaboral = filas.Sum(f => f.IgssLaboral),
            Isr = filas.Sum(f => f.Isr),
            OtrosDescuentos = filas.Sum(f => f.OtrosDescuentos),
            TotalDescuentos = filas.Sum(f => f.TotalDescuentos),
            LiquidoRecibir = filas.Sum(f => f.LiquidoRecibir)
        };
    }

    private static BoletaDto ABoleta(NominaEmpleado f)
    {
        var rubros = f.NominaRubro.OrderBy(r => r.Orden).ToList();

        return new BoletaDto
        {
            IdEmpleado = f.IdEmpleado,
            CodigoEmpleado = f.IdEmpleadoNavigation.CodigoEmpleado,
            NombreCompleto = NombreCompleto(f.IdEmpleadoNavigation),
            NombrePuesto = f.IdPuestoNavigation.Nombre,
            NombreDepartamento = f.IdDepartamentoNavigation.Nombre,
            TipoPago = f.TipoPago,
            DiasPagar = f.DiasPagar,
            Ingresos = rubros.Where(r => r.IdRubroNavigation.Tipo == "INGRESO").Select(r => ALinea(f, r)).ToList(),
            Descuentos = rubros.Where(r => r.IdRubroNavigation.Tipo == "DESCUENTO").Select(r => ALinea(f, r)).ToList(),
            TotalIngresos = f.TotalIngresos,
            TotalDescuentos = f.TotalDescuentos,
            LiquidoRecibir = f.Liquido
        };
    }

    private static LineaBoletaDto ALinea(NominaEmpleado f, NominaRubro r)
    {
        var detalle = r.IdRubroNavigation.Codigo switch
        {
            CodigosRubro.SalarioOrdinario => f.DiasPagar == 1 ? "1 día" : $"{f.DiasPagar} días",
            CodigosRubro.HorasExtra => $"{f.HorasExtra.ToString("0.##", CultureInfo.InvariantCulture)} horas",
            _ => r.Descripcion
        };

        return Linea(r.IdRubroNavigation.Nombre, detalle, r.Monto);
    }

    private static decimal Monto(NominaEmpleado f, string codigo)
    {
        return f.NominaRubro.Where(r => r.IdRubroNavigation.Codigo == codigo).Sum(r => r.Monto);
    }

    private static LineaBoletaDto Linea(string concepto, string detalle, decimal monto)
    {
        return new LineaBoletaDto { Concepto = concepto, Detalle = detalle, Monto = monto };
    }

    private static string NombreCompleto(Empleado empleado)
    {
        return $"{empleado.Nombres} {empleado.Apellidos}";
    }
}
