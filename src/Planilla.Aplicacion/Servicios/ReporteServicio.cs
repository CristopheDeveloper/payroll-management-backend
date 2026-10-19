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
        var nomina = await ObtenerCalculada(idNomina);
        var detalle = await _nominaRepositorio.ObtenerDetalle(idNomina);

        var departamentos = detalle
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
            IgssPatronal = detalle.Sum(f => f.IgssPatronal),
            Irtra = detalle.Sum(f => f.Irtra),
            Intecap = detalle.Sum(f => f.Intecap)
        };
        cuota.Total = cuota.IgssPatronal + cuota.Irtra + cuota.Intecap;

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
        var nomina = await ObtenerCalculada(idNomina);
        var detalle = (await _nominaRepositorio.ObtenerDetalle(idNomina))
            .Where(f => idEmpleado == null || f.IdEmpleado == idEmpleado)
            .OrderBy(f => f.IdDepartamentoNavigation.Nombre)
            .ToList();
        if (idEmpleado != null && detalle.Count == 0)
        {
            throw new ReglaNegocioException("El empleado no está en la planilla.", 404);
        }

        var movimientos = (await _nominaRepositorio.ObtenerMovimientos(idNomina, idEmpleado))
            .ToLookup(m => m.IdEmpleado);

        if (detalle.Any(f => !CoincidenMovimientos(f, movimientos[f.IdEmpleado])))
        {
            throw new ReglaNegocioException("Los movimientos cambiaron después del último cálculo. Recalcule la planilla.", 409);
        }

        return new ReporteBoletasDto
        {
            Encabezado = await CrearEncabezado(nomina),
            Boletas = detalle.Select(f => ABoleta(f, movimientos[f.IdEmpleado])).ToList()
        };
    }

    private async Task<Nomina> ObtenerCalculada(int idNomina)
    {
        var nomina = await _nominaRepositorio.ObtenerPorId(idNomina)
            ?? throw new ReglaNegocioException("La planilla no existe.", 404);
        if (nomina.Estado == "BORRADOR")
        {
            throw new ReglaNegocioException("Calcule la planilla antes de generar reportes.", 409);
        }
        return nomina;
    }

    private async Task<EncabezadoReporteDto> CrearEncabezado(Nomina nomina)
    {
        return new EncabezadoReporteDto
        {
            IdNomina = nomina.IdNomina,
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

    private static FilaPlanillaDto AFila(NominaDetalle f)
    {
        return new FilaPlanillaDto
        {
            IdEmpleado = f.IdEmpleado,
            CodigoEmpleado = f.IdEmpleadoNavigation.CodigoEmpleado,
            NombreCompleto = NombreCompleto(f.IdEmpleadoNavigation),
            NombrePuesto = f.IdPuestoNavigation.Nombre,
            DiasPagar = f.DiasPagar,
            HorasExtra = f.HorasExtra,
            SalarioDevengado = f.SalarioDevengado,
            MontoHorasExtra = f.MontoHorasExtra,
            Bonificacion = f.Bonificacion,
            OtrosIngresos = f.OtrosIngresosAfectos + f.OtrosIngresosNoAfectos,
            TotalIngresos = f.TotalIngresos,
            IgssLaboral = f.IgssLaboral,
            Isr = f.Isr,
            OtrosDescuentos = f.OtrosDescuentos,
            TotalDescuentos = f.TotalDescuentos,
            LiquidoRecibir = f.LiquidoRecibir
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

    private static bool CoincidenMovimientos(NominaDetalle f, IEnumerable<MovimientoNomina> movimientos)
    {
        var lista = movimientos.ToList();
        return lista.Where(m => m.IdRubroNavigation.Tipo == "INGRESO" && m.IdRubroNavigation.AfectoIgss).Sum(m => m.Monto) == f.OtrosIngresosAfectos
            && lista.Where(m => m.IdRubroNavigation.Tipo == "INGRESO" && !m.IdRubroNavigation.AfectoIgss).Sum(m => m.Monto) == f.OtrosIngresosNoAfectos
            && lista.Where(m => m.IdRubroNavigation.Tipo == "DESCUENTO").Sum(m => m.Monto) == f.OtrosDescuentos;
    }

    private static BoletaDto ABoleta(NominaDetalle f, IEnumerable<MovimientoNomina> movimientos)
    {
        var ingresos = new List<LineaBoletaDto>
        {
            Linea("Salario devengado", f.DiasPagar == 1 ? "1 día" : $"{f.DiasPagar} días", f.SalarioDevengado)
        };
        if (f.HorasExtra > 0 || f.MontoHorasExtra > 0)
        {
            ingresos.Add(Linea("Horas extra", $"{f.HorasExtra.ToString("0.##", CultureInfo.InvariantCulture)} horas", f.MontoHorasExtra));
        }
        ingresos.Add(Linea("Bonificación incentivo", null, f.Bonificacion));
        ingresos.AddRange(movimientos
            .Where(m => m.IdRubroNavigation.Tipo == "INGRESO")
            .Select(m => Linea(m.IdRubroNavigation.Nombre, m.Descripcion, m.Monto)));

        var descuentos = new List<LineaBoletaDto>
        {
            Linea("IGSS laboral", null, f.IgssLaboral)
        };
        if (f.Isr > 0)
        {
            descuentos.Add(Linea("ISR", null, f.Isr));
        }
        descuentos.AddRange(movimientos
            .Where(m => m.IdRubroNavigation.Tipo == "DESCUENTO")
            .Select(m => Linea(m.IdRubroNavigation.Nombre, m.Descripcion, m.Monto)));

        return new BoletaDto
        {
            IdEmpleado = f.IdEmpleado,
            CodigoEmpleado = f.IdEmpleadoNavigation.CodigoEmpleado,
            NombreCompleto = NombreCompleto(f.IdEmpleadoNavigation),
            NombrePuesto = f.IdPuestoNavigation.Nombre,
            NombreDepartamento = f.IdDepartamentoNavigation.Nombre,
            TipoPago = f.TipoPago,
            DiasPagar = f.DiasPagar,
            Ingresos = ingresos,
            Descuentos = descuentos,
            TotalIngresos = f.TotalIngresos,
            TotalDescuentos = f.TotalDescuentos,
            LiquidoRecibir = f.LiquidoRecibir
        };
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
