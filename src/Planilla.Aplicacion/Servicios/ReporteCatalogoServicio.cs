using System.Globalization;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Excepciones;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Constantes;
using Planilla.Dominio.Entidades;
using Planilla.Dominio.Tiempo;

namespace Planilla.Aplicacion.Servicios;

public class ReporteCatalogoServicio : IReporteCatalogoServicio
{
    private const string Texto = "TEXTO";
    private const string Numero = "NUMERO";
    private const string Moneda = "MONTO";
    private const string Fecha = "FECHA";

    private static readonly string[] Meses =
    [
        "enero", "febrero", "marzo", "abril", "mayo", "junio",
        "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"
    ];

    private static readonly Dictionary<string, string> TiposPago = new()
    {
        ["TRANSFERENCIA"] = "Transferencia",
        ["CHEQUE"] = "Cheque",
        ["EFECTIVO"] = "Efectivo"
    };

    private static readonly Dictionary<string, string> TiposBaja = new()
    {
        ["RENUNCIA"] = "Renuncia",
        ["DESPIDO"] = "Despido",
        ["FIN_CONTRATO"] = "Fin de contrato",
        ["OTRO"] = "Otro"
    };

    private static readonly Dictionary<string, string> EstadosAsistencia = new()
    {
        ["FALTA"] = "F",
        ["PERMISO"] = "P",
        ["SUSPENSION_IGSS"] = "S"
    };

    private readonly INominaRepositorio _nominaRepositorio;
    private readonly IEmpleadoRepositorio _empleadoRepositorio;
    private readonly IAsistenciaRepositorio _asistenciaRepositorio;
    private readonly IRubroRepositorio _rubroRepositorio;
    private readonly IDepartamentoRepositorio _departamentoRepositorio;
    private readonly IPuestoRepositorio _puestoRepositorio;
    private readonly IParametroRepositorio _parametroRepositorio;

    public ReporteCatalogoServicio(INominaRepositorio nominaRepositorio, IEmpleadoRepositorio empleadoRepositorio,
        IAsistenciaRepositorio asistenciaRepositorio, IRubroRepositorio rubroRepositorio,
        IDepartamentoRepositorio departamentoRepositorio, IPuestoRepositorio puestoRepositorio,
        IParametroRepositorio parametroRepositorio)
    {
        _nominaRepositorio = nominaRepositorio;
        _empleadoRepositorio = empleadoRepositorio;
        _asistenciaRepositorio = asistenciaRepositorio;
        _rubroRepositorio = rubroRepositorio;
        _departamentoRepositorio = departamentoRepositorio;
        _puestoRepositorio = puestoRepositorio;
        _parametroRepositorio = parametroRepositorio;
    }

    public async Task<ReporteTablaDto> PlanillaGeneral(int idNomina)
    {
        var nomina = await ObtenerGenerada(idNomina);
        var empleados = await _nominaRepositorio.ObtenerEmpleadosNomina(idNomina);

        var columnas = new List<ColumnaReporteDto>
        {
            Columna("Código"), Columna("Empleado"), Columna("Puesto"), Columna("Días", Numero),
            Columna("Salario", Moneda, true), Columna("Horas extra", Moneda, true), Columna("Bonificación", Moneda, true),
            Columna("Otros ingresos", Moneda, true), Columna("Total ingresos", Moneda, true), Columna("IGSS", Moneda, true),
            Columna("ISR", Moneda, true), Columna("Otros descuentos", Moneda, true),
            Columna("Total descuentos", Moneda, true), Columna("Líquido", Moneda, true)
        };
        var filas = empleados.Select(f =>
        {
            var salario = Monto(f, CodigosRubro.SalarioOrdinario);
            var horasExtra = Monto(f, CodigosRubro.HorasExtra);
            var bonificacion = Monto(f, CodigosRubro.BonificacionIncentivo);
            var igss = Monto(f, CodigosRubro.IgssLaboral);
            var isr = Monto(f, CodigosRubro.Isr);
            return Fila(f.IdDepartamentoNavigation.Nombre, f.IdEmpleadoNavigation.CodigoEmpleado,
                NombreCompleto(f.IdEmpleadoNavigation), f.IdPuestoNavigation.Nombre, f.DiasPagar, salario, horasExtra,
                bonificacion, f.TotalIngresos - salario - horasExtra - bonificacion, f.TotalIngresos, igss, isr,
                f.TotalDescuentos - igss - isr, f.TotalDescuentos, f.Liquido);
        }).ToList();

        return await Crear("Planilla general", Subtitulo(nomina), [], columnas, filas,
            $"planilla-{nomina.Codigo}", Preliminar(nomina));
    }

    public async Task<ReporteTablaDto> Boletas(int idNomina)
    {
        var nomina = await ObtenerGenerada(idNomina);
        var empleados = await _nominaRepositorio.ObtenerEmpleadosNomina(idNomina);

        var columnas = new List<ColumnaReporteDto>
        {
            Columna("Código"), Columna("Empleado"), Columna("Puesto"), Columna("Forma de pago"),
            Columna("Días", Numero), Columna("Ingresos", Moneda, true), Columna("Descuentos", Moneda, true),
            Columna("Líquido", Moneda, true)
        };
        var filas = empleados.Select(f => Fila(null, f.IdEmpleadoNavigation.CodigoEmpleado,
            NombreCompleto(f.IdEmpleadoNavigation), f.IdPuestoNavigation.Nombre, TipoPago(f.TipoPago), f.DiasPagar,
            f.TotalIngresos, f.TotalDescuentos, f.Liquido)).ToList();

        return await Crear("Boletas de pago", Subtitulo(nomina), [], columnas, filas,
            $"boletas-{nomina.Codigo}", Preliminar(nomina));
    }

    public async Task<ReporteTablaDto> FormaPago(int idNomina, string tipoPago)
    {
        var tipo = string.IsNullOrWhiteSpace(tipoPago) ? "TRANSFERENCIA" : tipoPago.Trim().ToUpper();
        if (!TiposPago.ContainsKey(tipo))
        {
            throw new ReglaNegocioException("La forma de pago debe ser TRANSFERENCIA, CHEQUE o EFECTIVO.");
        }

        var nomina = await ObtenerGenerada(idNomina);
        var empleados = (await _nominaRepositorio.ObtenerEmpleadosNomina(idNomina))
            .Where(f => f.TipoPago == tipo)
            .ToList();

        List<ColumnaReporteDto> columnas;
        List<FilaReporteDto> filas;
        if (tipo == "TRANSFERENCIA")
        {
            columnas =
            [
                Columna("Cuenta"), Columna("Código"), Columna("Empleado"), Columna("DPI"), Columna("Monto", Moneda, true)
            ];
            filas = empleados
                .OrderBy(f => f.Banco)
                .ThenBy(f => f.IdEmpleadoNavigation.Apellidos)
                .Select(f => Fila(f.Banco ?? "Sin banco", f.CuentaBancaria, f.IdEmpleadoNavigation.CodigoEmpleado,
                    NombreCompleto(f.IdEmpleadoNavigation), f.IdEmpleadoNavigation.Dpi, f.Liquido))
                .ToList();
        }
        else
        {
            columnas =
            [
                Columna("Código"), Columna("Empleado"), Columna("DPI"), Columna("Departamento"), Columna("Monto", Moneda, true)
            ];
            filas = empleados
                .OrderBy(f => f.IdEmpleadoNavigation.Apellidos)
                .Select(f => Fila(null, f.IdEmpleadoNavigation.CodigoEmpleado, NombreCompleto(f.IdEmpleadoNavigation),
                    f.IdEmpleadoNavigation.Dpi, f.IdDepartamentoNavigation.Nombre, f.Liquido))
                .ToList();
        }

        return await Crear($"Pagos por {TiposPago[tipo].ToLower()}", Subtitulo(nomina), [], columnas, filas,
            $"pagos-{tipo.ToLower()}-{nomina.Codigo}", Preliminar(nomina));
    }

    public async Task<ReporteTablaDto> Rubros(int idNomina, int? idRubro)
    {
        var nomina = await ObtenerGenerada(idNomina);
        var empleados = await _nominaRepositorio.ObtenerEmpleadosNomina(idNomina);

        if (idRubro == null)
        {
            var resumen = empleados
                .SelectMany(f => f.NominaRubro)
                .Where(r => r.Monto != 0)
                .GroupBy(r => r.IdRubro)
                .Select(g => new
                {
                    Rubro = g.First().IdRubroNavigation,
                    Empleados = g.Select(r => r.IdNominaEmpleado).Distinct().Count(),
                    Total = g.Sum(r => r.Monto)
                })
                .OrderBy(x => x.Rubro.Orden)
                .ToList();
            var columnasResumen = new List<ColumnaReporteDto>
            {
                Columna("Rubro"), Columna("Empleados", Numero), Columna("Total", Moneda)
            };
            var filasResumen = resumen
                .Select(x => Fila(TipoRubro(x.Rubro.Tipo), x.Rubro.Nombre, x.Empleados, x.Total))
                .ToList();
            return await Crear("Rubros de la planilla", Subtitulo(nomina), ["Todos los rubros"], columnasResumen,
                filasResumen, $"rubros-{nomina.Codigo}", Preliminar(nomina));
        }

        var rubro = await _rubroRepositorio.ObtenerPorId(idRubro.Value)
            ?? throw new ReglaNegocioException("El rubro no existe.", 404);
        var columnas = new List<ColumnaReporteDto>
        {
            Columna("Código"), Columna("Empleado"), Columna("Base", Moneda), Columna("Monto", Moneda, true),
            Columna("Descripción")
        };
        var filas = empleados
            .SelectMany(f => f.NominaRubro.Where(r => r.IdRubro == rubro.IdRubro && r.Monto != 0).Select(r => new { Fila = f, Rubro = r }))
            .Select(x => Fila(x.Fila.IdDepartamentoNavigation.Nombre, x.Fila.IdEmpleadoNavigation.CodigoEmpleado,
                NombreCompleto(x.Fila.IdEmpleadoNavigation), x.Rubro.Base, x.Rubro.Monto, x.Rubro.Descripcion))
            .ToList();

        return await Crear($"Rubro: {rubro.Nombre}", Subtitulo(nomina), [], columnas, filas,
            $"rubro-{rubro.Codigo.ToLower()}-{nomina.Codigo}", Preliminar(nomina));
    }

    public async Task<ReporteTablaDto> Igss(int idNomina)
    {
        var nomina = await ObtenerGenerada(idNomina);
        var empleados = await _nominaRepositorio.ObtenerEmpleadosNomina(idNomina);

        var columnas = new List<ColumnaReporteDto>
        {
            Columna("Código"), Columna("Empleado"), Columna("No. afiliación"), Columna("Salario afecto", Moneda, true),
            Columna($"Laboral ({Porcentaje(nomina.IgssLaboral)})", Moneda, true),
            Columna($"Patronal ({Porcentaje(nomina.IgssPatronal)})", Moneda, true), Columna("Total", Moneda, true)
        };
        var filas = empleados.Select(f =>
        {
            var laboral = f.NominaRubro.FirstOrDefault(r => r.IdRubroNavigation.Codigo == CodigosRubro.IgssLaboral);
            var patronal = Monto(f, CodigosRubro.IgssPatronal);
            var montoLaboral = laboral?.Monto ?? 0;
            return Fila(null, f.IdEmpleadoNavigation.CodigoEmpleado, NombreCompleto(f.IdEmpleadoNavigation),
                f.IdEmpleadoNavigation.NumeroIgss, laboral?.Base ?? 0, montoLaboral, patronal, montoLaboral + patronal);
        }).ToList();

        return await Crear("Cuota IGSS", Subtitulo(nomina), [], columnas, filas, $"igss-{nomina.Codigo}",
            Preliminar(nomina));
    }

    public async Task<ReporteTablaDto> Poliza(int idNomina)
    {
        var nomina = await ObtenerGenerada(idNomina);
        var empleados = await _nominaRepositorio.ObtenerEmpleadosNomina(idNomina);
        var rubros = empleados
            .SelectMany(f => f.NominaRubro)
            .Where(r => r.Monto != 0)
            .GroupBy(r => r.IdRubro)
            .Select(g => new { Rubro = g.First().IdRubroNavigation, Total = g.Sum(r => r.Monto) })
            .OrderBy(x => x.Rubro.Orden)
            .ToList();

        var sinCuenta = rubros.Where(x => string.IsNullOrWhiteSpace(x.Rubro.CuentaContable)).Select(x => x.Rubro.Nombre).ToList();
        if (sinCuenta.Count > 0)
        {
            throw new ReglaNegocioException($"Asigne la cuenta contable en Rubros a: {string.Join(", ", sinCuenta)}.", 409);
        }
        var sueldosPorPagar = await CuentaParametro(ClavesParametro.CuentaSueldosPorPagar, "sueldos por pagar");
        var igssPatronalPorPagar = await CuentaParametro(ClavesParametro.CuentaIgssPatronalPorPagar, "IGSS patronal por pagar");

        var lineas = new List<(string Cuenta, string Nombre, decimal Debe, decimal Haber)>();
        foreach (var x in rubros)
        {
            var cuenta = x.Rubro.CuentaContable.Trim();
            if (x.Rubro.Tipo == "DESCUENTO")
            {
                lineas.Add((cuenta, x.Rubro.Nombre, 0, x.Total));
            }
            else
            {
                lineas.Add((cuenta, x.Rubro.Nombre, x.Total, 0));
            }
            if (x.Rubro.Tipo == "PATRONAL")
            {
                lineas.Add((igssPatronalPorPagar, "IGSS patronal por pagar", 0, x.Total));
            }
        }
        lineas.Add((sueldosPorPagar, "Sueldos por pagar", 0, empleados.Sum(f => f.Liquido)));

        var columnas = new List<ColumnaReporteDto>
        {
            Columna("Cuenta"), Columna("Descripción"), Columna("Debe", Moneda, true), Columna("Haber", Moneda, true)
        };
        var filas = lineas
            .GroupBy(l => l.Cuenta)
            .Select(g => new
            {
                Cuenta = g.Key,
                Nombre = string.Join(", ", g.Select(l => l.Nombre).Distinct()),
                Debe = g.Sum(l => l.Debe),
                Haber = g.Sum(l => l.Haber)
            })
            .OrderBy(c => c.Debe > 0 ? 0 : 1)
            .ThenBy(c => c.Cuenta)
            .Select(c => Fila(null, c.Cuenta, c.Nombre, c.Debe == 0 ? null : c.Debe, c.Haber == 0 ? null : c.Haber))
            .ToList();

        return await Crear("Póliza contable", Subtitulo(nomina), [], columnas, filas, $"poliza-{nomina.Codigo}",
            Preliminar(nomina));
    }

    public async Task<ReporteTablaDto> PolizaPago(int idNomina)
    {
        var nomina = await ObtenerGenerada(idNomina);
        var empleados = await _nominaRepositorio.ObtenerEmpleadosNomina(idNomina);
        var liquido = empleados.Sum(f => f.Liquido);
        var efectivo = empleados.Where(f => f.TipoPago == "EFECTIVO").Sum(f => f.Liquido);
        var banco = liquido - efectivo;

        var lineas = new List<(string Cuenta, string Nombre, decimal Debe, decimal Haber)>
        {
            (await CuentaParametro(ClavesParametro.CuentaSueldosPorPagar, "sueldos por pagar"), "Sueldos por pagar",
                liquido, 0)
        };
        if (banco != 0)
        {
            lineas.Add((await CuentaParametro(ClavesParametro.CuentaBancos, "bancos"),
                "Bancos (transferencias y cheques)", 0, banco));
        }
        if (efectivo != 0)
        {
            lineas.Add((await CuentaParametro(ClavesParametro.CuentaCaja, "caja"), "Caja (efectivo)", 0, efectivo));
        }

        var columnas = new List<ColumnaReporteDto>
        {
            Columna("Cuenta"), Columna("Descripción"), Columna("Debe", Moneda, true), Columna("Haber", Moneda, true)
        };
        var filas = lineas
            .Select(l => Fila(null, l.Cuenta, l.Nombre, l.Debe == 0 ? null : l.Debe, l.Haber == 0 ? null : l.Haber))
            .ToList();

        return await Crear("Póliza de pago de sueldos", Subtitulo(nomina), [], columnas, filas,
            $"poliza-pago-{nomina.Codigo}", Preliminar(nomina));
    }

    public async Task<ReporteTablaDto> Empleados(string estado, int? idDepartamento, int? idPuesto)
    {
        var valor = string.IsNullOrWhiteSpace(estado) ? "ACTIVOS" : estado.Trim().ToUpper();
        bool? activo = valor switch
        {
            "ACTIVOS" => true,
            "BAJA" => false,
            "TODOS" => null,
            _ => throw new ReglaNegocioException("El estado debe ser ACTIVOS, BAJA o TODOS.")
        };

        var empleados = await _empleadoRepositorio.ObtenerTodos(activo, idDepartamento, idPuesto, null);
        var filtros = new List<string>
        {
            valor switch { "ACTIVOS" => "Empleados activos", "BAJA" => "Empleados de baja", _ => "Todos los empleados" }
        };
        if (idDepartamento.HasValue)
        {
            filtros.Add($"Departamento: {(await _departamentoRepositorio.ObtenerPorId(idDepartamento.Value))?.Nombre}");
        }
        if (idPuesto.HasValue)
        {
            filtros.Add($"Puesto: {(await _puestoRepositorio.ObtenerPorId(idPuesto.Value))?.Nombre}");
        }

        var columnas = new List<ColumnaReporteDto>
        {
            Columna("Código"), Columna("Empleado"), Columna("DPI"), Columna("NIT", soloExcel: true),
            Columna("No. IGSS", soloExcel: true), Columna("Pasaporte", soloExcel: true),
            Columna("Nacimiento", Fecha, soloExcel: true), Columna("Género", soloExcel: true),
            Columna("Dirección", soloExcel: true), Columna("Teléfono"), Columna("Correo", soloExcel: true),
            Columna("Puesto"), Columna("Departamento"), Columna("Ingreso", Fecha), Columna("Retiro", Fecha),
            Columna("Salario", Moneda, true), Columna("Bonificación", Moneda, true), Columna("ISR", Moneda, soloExcel: true),
            Columna("Forma de pago"), Columna("Banco", soloExcel: true), Columna("Cuenta", soloExcel: true)
        };
        var filas = empleados
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .Select(e =>
            {
                var periodo = e.PeriodoLaboral.OrderByDescending(p => p.FechaIngreso).FirstOrDefault();
                return Fila(null, e.CodigoEmpleado, NombreCompleto(e), e.Dpi, e.Nit, e.NumeroIgss, e.Pasaporte,
                    e.FechaNacimiento, e.Genero == "F" ? "Femenino" : "Masculino", e.Direccion, e.Telefono, e.Correo,
                    e.IdPuestoNavigation.Nombre, e.IdDepartamentoNavigation.Nombre, periodo?.FechaIngreso,
                    periodo?.FechaBaja, e.Salario, e.BonificacionIncentivo, e.RetencionIsr, TipoPago(e.TipoPago), e.Banco,
                    e.CuentaBancaria);
            })
            .ToList();

        return await Crear("Listado de empleados", null, filtros, columnas, filas, "empleados", false);
    }

    public async Task<ReporteTablaDto> AltasBajas(DateOnly desde, DateOnly hasta)
    {
        ValidarRango(desde, hasta);
        var empleados = await _empleadoRepositorio.ObtenerTodos(null, null, null, null);

        var movimientos = empleados
            .SelectMany(e => e.PeriodoLaboral.SelectMany(p => new[]
            {
                p.FechaIngreso >= desde && p.FechaIngreso <= hasta
                    ? new { Fecha = p.FechaIngreso, Movimiento = "Alta", Empleado = e, Periodo = p }
                    : null,
                p.FechaBaja >= desde && p.FechaBaja <= hasta
                    ? new { Fecha = p.FechaBaja.Value, Movimiento = "Baja", Empleado = e, Periodo = p }
                    : null
            }))
            .Where(m => m != null)
            .OrderBy(m => m.Fecha)
            .ThenBy(m => m.Empleado.Apellidos)
            .ToList();

        var columnas = new List<ColumnaReporteDto>
        {
            Columna("Fecha", Fecha), Columna("Movimiento"), Columna("Código"), Columna("Empleado"), Columna("Puesto"),
            Columna("Departamento"), Columna("Tipo de retiro"), Columna("Motivo")
        };
        var filas = movimientos.Select(m => Fila(null, m.Fecha, m.Movimiento, m.Empleado.CodigoEmpleado,
            NombreCompleto(m.Empleado), m.Empleado.IdPuestoNavigation.Nombre, m.Empleado.IdDepartamentoNavigation.Nombre,
            m.Movimiento == "Baja" ? TipoBaja(m.Periodo.TipoBaja) : null,
            m.Movimiento == "Baja" ? m.Periodo.MotivoBaja : null)).ToList();

        var altas = movimientos.Count(m => m.Movimiento == "Alta");
        return await Crear("Altas y bajas", $"Del {desde:dd'/'MM'/'yyyy} al {hasta:dd'/'MM'/'yyyy}",
            [$"Altas: {altas}", $"Bajas: {movimientos.Count - altas}"], columnas, filas,
            $"altas-bajas-{desde:yyyyMMdd}-{hasta:yyyyMMdd}", false);
    }

    public async Task<ReporteTablaDto> Historial(int idEmpleado, DateOnly? desde, DateOnly? hasta)
    {
        if (desde.HasValue && hasta.HasValue)
        {
            ValidarRango(desde.Value, hasta.Value);
        }
        var empleado = await _empleadoRepositorio.ObtenerPorId(idEmpleado)
            ?? throw new ReglaNegocioException("El empleado no existe.", 404);
        var pagos = (await _nominaRepositorio.ObtenerPagosEmpleado(idEmpleado))
            .Where(p => (desde == null || p.IdNominaNavigation.FechaFin >= desde)
                && (hasta == null || p.IdNominaNavigation.FechaInicio <= hasta))
            .OrderBy(p => p.IdNominaNavigation.FechaInicio)
            .ThenBy(p => p.IdNominaNavigation.Codigo)
            .ToList();

        var columnas = new List<ColumnaReporteDto>
        {
            Columna("Planilla"), Columna("Periodo"), Columna("Días", Numero), Columna("Salario", Moneda, true),
            Columna("Ingresos", Moneda, true), Columna("Descuentos", Moneda, true), Columna("Líquido", Moneda, true)
        };
        var filas = pagos.Select(p => Fila(null, p.IdNominaNavigation.Codigo,
            $"{Meses[p.IdNominaNavigation.Mes - 1]} {p.IdNominaNavigation.Anio}", p.DiasPagar,
            Monto(p, CodigosRubro.SalarioOrdinario), p.TotalIngresos, p.TotalDescuentos, p.Liquido)).ToList();

        var filtros = new List<string> { "Solo planillas cerradas" };
        if (desde.HasValue && hasta.HasValue)
        {
            filtros.Add($"Del {desde:dd'/'MM'/'yyyy} al {hasta:dd'/'MM'/'yyyy}");
        }
        else if (desde.HasValue)
        {
            filtros.Add($"Desde {desde:dd'/'MM'/'yyyy}");
        }
        else if (hasta.HasValue)
        {
            filtros.Add($"Hasta {hasta:dd'/'MM'/'yyyy}");
        }
        return await Crear("Historial de pagos", $"{empleado.CodigoEmpleado} · {NombreCompleto(empleado)}", filtros,
            columnas, filas, $"historial-{empleado.CodigoEmpleado}", false);
    }

    public async Task<ReporteTablaDto> Asistencia(int anio, int mes, int? idDepartamento)
    {
        if (anio < 2000 || anio > 2100 || mes < 1 || mes > 12)
        {
            throw new ReglaNegocioException("El mes o el año no son válidos.");
        }
        var desde = new DateOnly(anio, mes, 1);
        var hasta = desde.AddMonths(1).AddDays(-1);

        var empleados = (await _asistenciaRepositorio.ObtenerEmpleadosConPeriodoEntre(desde, hasta))
            .Where(e => idDepartamento == null || e.IdDepartamento == idDepartamento)
            .ToList();
        var ausencias = (await _asistenciaRepositorio.ObtenerAusenciasEntre(desde, hasta))
            .GroupBy(a => a.IdEmpleado)
            .ToDictionary(g => g.Key, g => g.ToList());

        var columnas = new List<ColumnaReporteDto>
        {
            Columna("Código"), Columna("Empleado"), Columna("Faltas", Numero, true), Columna("Permisos", Numero, true),
            Columna("Suspensión IGSS", Numero, true), Columna("Fechas")
        };
        var filas = empleados
            .OrderBy(e => e.IdDepartamentoNavigation.Nombre)
            .ThenBy(e => e.Apellidos)
            .Select(e =>
            {
                var registros = ausencias.GetValueOrDefault(e.IdEmpleado) ?? [];
                var fechas = string.Join(", ", registros.Select(a => $"{a.Fecha.Day} ({EstadosAsistencia[a.Estado]})"));
                return Fila(e.IdDepartamentoNavigation.Nombre, e.CodigoEmpleado, NombreCompleto(e),
                    registros.Count(a => a.Estado == "FALTA"), registros.Count(a => a.Estado == "PERMISO"),
                    registros.Count(a => a.Estado == "SUSPENSION_IGSS"), fechas);
            })
            .ToList();

        var filtros = new List<string> { "F = falta, P = permiso, S = suspensión IGSS" };
        if (idDepartamento.HasValue)
        {
            filtros.Insert(0, $"Departamento: {(await _departamentoRepositorio.ObtenerPorId(idDepartamento.Value))?.Nombre}");
        }
        return await Crear("Asistencia del mes", $"{Capitalizar(Meses[mes - 1])} {anio}", filtros, columnas, filas,
            $"asistencia-{anio}-{mes:00}", false);
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

    private async Task<ReporteTablaDto> Crear(string titulo, string subtitulo, List<string> filtros,
        List<ColumnaReporteDto> columnas, List<FilaReporteDto> filas, string archivo, bool preliminar)
    {
        return new ReporteTablaDto
        {
            Titulo = titulo,
            Subtitulo = subtitulo,
            Filtros = filtros,
            NombreEmpresa = (await _parametroRepositorio.ObtenerPorClave(ClavesParametro.NombreEmpresa))?.Valor,
            NitEmpresa = (await _parametroRepositorio.ObtenerPorClave(ClavesParametro.NitEmpresa))?.Valor,
            DireccionEmpresa = (await _parametroRepositorio.ObtenerPorClave(ClavesParametro.DireccionEmpresa))?.Valor,
            FechaGeneracion = RelojGuatemala.Ahora,
            Preliminar = preliminar,
            NombreArchivo = archivo,
            Columnas = columnas,
            Filas = filas
        };
    }

    private async Task<string> CuentaParametro(string clave, string nombre)
    {
        var valor = (await _parametroRepositorio.ObtenerPorClave(clave))?.Valor?.Trim();
        if (valor == null || valor.Length != 4 || !valor.All(char.IsAsciiDigit))
        {
            throw new ReglaNegocioException($"Configure la cuenta de {nombre} en Configuración.", 409);
        }
        return valor;
    }

    private static void ValidarRango(DateOnly desde, DateOnly hasta)
    {
        if (hasta < desde)
        {
            throw new ReglaNegocioException("La fecha final no puede ser anterior a la fecha inicial.");
        }
    }

    private static ColumnaReporteDto Columna(string titulo, string tipo = Texto, bool sumar = false, bool soloExcel = false)
    {
        return new ColumnaReporteDto { Titulo = titulo, Tipo = tipo, Sumar = sumar, SoloExcel = soloExcel };
    }

    private static FilaReporteDto Fila(string grupo, params object[] valores)
    {
        return new FilaReporteDto { Grupo = grupo, Valores = valores.ToList() };
    }

    private static decimal Monto(NominaEmpleado f, string codigo)
    {
        return f.NominaRubro.Where(r => r.IdRubroNavigation.Codigo == codigo).Sum(r => r.Monto);
    }

    private static string Subtitulo(Nomina nomina)
    {
        return $"{nomina.Codigo} · {Capitalizar(Meses[nomina.Mes - 1])} {nomina.Anio}";
    }

    private static bool Preliminar(Nomina nomina)
    {
        return nomina.Estado != "CERRADA";
    }

    private static string Porcentaje(decimal? valor)
    {
        return $"{valor?.ToString("0.##", CultureInfo.InvariantCulture)} %";
    }

    private static string TipoPago(string tipo)
    {
        return tipo != null && TiposPago.TryGetValue(tipo, out var nombre) ? nombre : tipo;
    }

    private static string TipoBaja(string tipo)
    {
        return tipo != null && TiposBaja.TryGetValue(tipo, out var nombre) ? nombre : tipo;
    }

    private static string TipoRubro(string tipo)
    {
        return tipo switch { "INGRESO" => "Ingresos", "DESCUENTO" => "Descuentos", _ => "Patronal" };
    }

    private static string Capitalizar(string texto)
    {
        return char.ToUpper(texto[0]) + texto[1..];
    }

    private static string NombreCompleto(Empleado empleado)
    {
        return $"{empleado.Nombres} {empleado.Apellidos}";
    }
}
