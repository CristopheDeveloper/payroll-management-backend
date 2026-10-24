using System.Globalization;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Excepciones;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Calculo;
using Planilla.Dominio.Constantes;
using Planilla.Dominio.Entidades;
using Planilla.Dominio.Tiempo;

namespace Planilla.Aplicacion.Servicios;

public class NominaServicio : INominaServicio
{
    private readonly INominaRepositorio _nominaRepositorio;
    private readonly IAsistenciaRepositorio _asistenciaRepositorio;
    private readonly IParametroRepositorio _parametroRepositorio;
    private readonly IPuestoRepositorio _puestoRepositorio;
    private readonly IDepartamentoRepositorio _departamentoRepositorio;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IUnidadTrabajo _unidadTrabajo;

    public NominaServicio(INominaRepositorio nominaRepositorio, IAsistenciaRepositorio asistenciaRepositorio,
        IParametroRepositorio parametroRepositorio, IPuestoRepositorio puestoRepositorio,
        IDepartamentoRepositorio departamentoRepositorio, IUsuarioActual usuarioActual, IUnidadTrabajo unidadTrabajo)
    {
        _nominaRepositorio = nominaRepositorio;
        _asistenciaRepositorio = asistenciaRepositorio;
        _parametroRepositorio = parametroRepositorio;
        _puestoRepositorio = puestoRepositorio;
        _departamentoRepositorio = departamentoRepositorio;
        _usuarioActual = usuarioActual;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<List<NominaDto>> ObtenerTodas(int? anio)
    {
        var nominas = await _nominaRepositorio.ObtenerTodas(anio);
        var cantidades = await _nominaRepositorio.ContarEmpleados(nominas.Select(n => n.IdNomina).ToList());

        return nominas.Select(n => ADto(n, cantidades.GetValueOrDefault(n.IdNomina), null)).ToList();
    }

    public async Task<NominaDto> ObtenerPorId(int idNomina)
    {
        var nomina = await ObtenerExistente(idNomina);
        var detalle = await _nominaRepositorio.ObtenerDetalle(idNomina);
        var codigos = detalle.ToDictionary(f => f.IdEmpleado, f => f.IdEmpleadoNavigation.CodigoEmpleado);
        var advertencias = await CalcularAdvertencias(nomina, detalle, codigos);

        return ADto(nomina, detalle.Count, advertencias);
    }

    public async Task<NominaDto> Crear(NominaCrearDto dto)
    {
        var inicio = new DateOnly(dto.Anio, dto.Mes, 1);
        var hoy = Hoy();
        if (inicio > new DateOnly(hoy.Year, hoy.Month, 1).AddMonths(1))
        {
            throw new ReglaNegocioException("Solo se pueden crear planillas hasta el mes siguiente.");
        }
        if (await _nominaRepositorio.Existe(dto.Anio, dto.Mes))
        {
            throw new ReglaNegocioException($"Ya existe la planilla de {dto.Mes:00}/{dto.Anio}.", 409);
        }

        var masAntigua = await _nominaRepositorio.ObtenerInicioMasAntiguo();
        if (masAntigua.HasValue && inicio < masAntigua.Value)
        {
            throw new ReglaNegocioException($"No se pueden crear planillas anteriores a la más antigua ({masAntigua.Value:MM'/'yyyy}).", 409);
        }

        var nomina = new Nomina
        {
            Anio = dto.Anio,
            Mes = dto.Mes,
            FechaInicio = inicio,
            FechaFin = inicio.AddMonths(1).AddDays(-1),
            Estado = "BORRADOR"
        };

        _nominaRepositorio.Agregar(nomina);
        await _unidadTrabajo.GuardarCambiosAsync();

        return await ObtenerPorId(nomina.IdNomina);
    }

    public async Task Eliminar(int idNomina)
    {
        var nomina = await _nominaRepositorio.ObtenerParaModificar(idNomina)
            ?? throw new ReglaNegocioException("La planilla no existe.", 404);

        if (nomina.Estado == "CERRADA")
        {
            throw new ReglaNegocioException("Reabra la planilla antes de eliminarla.", 409);
        }

        foreach (var fila in await _nominaRepositorio.ObtenerDetalleParaModificar(idNomina))
        {
            _nominaRepositorio.EliminarDetalle(fila);
        }
        foreach (var movimiento in await _nominaRepositorio.ObtenerMovimientosParaModificar(idNomina))
        {
            _nominaRepositorio.EliminarMovimiento(movimiento);
        }
        _nominaRepositorio.Eliminar(nomina);

        await _unidadTrabajo.GuardarCambiosAsync();
    }

    public async Task<List<EmpleadoMesDto>> ObtenerEmpleados(int idNomina)
    {
        var nomina = await ObtenerExistente(idNomina);
        var empleados = await _asistenciaRepositorio.ObtenerEmpleadosConPeriodoEntre(nomina.FechaInicio, nomina.FechaFin);

        return empleados.Select(e => new EmpleadoMesDto
        {
            IdEmpleado = e.IdEmpleado,
            CodigoEmpleado = e.CodigoEmpleado,
            NombreCompleto = $"{e.Nombres} {e.Apellidos}"
        }).ToList();
    }

    public async Task<List<NominaDetalleDto>> ObtenerDetalle(int idNomina)
    {
        var nomina = await ObtenerExistente(idNomina);
        var detalle = await _nominaRepositorio.ObtenerDetalle(idNomina);

        return detalle.Select(f => ADetalleDto(f, nomina.Estado == "CERRADA"
            ? new List<AdvertenciaDto>()
            : AdvertenciasFila(f, nomina.DiasMes, f.IdEmpleadoNavigation.CodigoEmpleado))).ToList();
    }

    public async Task<ComparacionDto> ObtenerComparacion(int idNomina)
    {
        var nomina = await ObtenerExistente(idNomina);
        if (nomina.Estado == "CERRADA")
        {
            throw new ReglaNegocioException("La planilla está cerrada.", 409);
        }
        if (nomina.FechaCalculo == null)
        {
            return new ComparacionDto { Diferencias = [] };
        }

        var actuales = await LeerParametros();
        var datos = await CargarDatosMes(nomina);
        var detalle = await _nominaRepositorio.ObtenerDetalleParaModificar(idNomina);
        var diferencias = BuscarDiferencias(nomina, detalle, datos.Empleados, actuales);

        var puestos = (await _puestoRepositorio.ObtenerTodos(false)).ToDictionary(p => p.IdPuesto.ToString(), p => p.Nombre);
        var departamentos = (await _departamentoRepositorio.ObtenerTodos(false)).ToDictionary(d => d.IdDepartamento.ToString(), d => d.Nombre);

        foreach (var diferencia in diferencias)
        {
            if (diferencia.IdEmpleado.HasValue)
            {
                var empleado = datos.Empleados[diferencia.IdEmpleado.Value];
                diferencia.CodigoEmpleado = empleado.CodigoEmpleado;
                diferencia.NombreCompleto = $"{empleado.Nombres} {empleado.Apellidos}";
            }

            var nombres = diferencia.Campo == CalculoNomina.CampoIdPuesto ? puestos
                : diferencia.Campo == CalculoNomina.CampoIdDepartamento ? departamentos
                : null;
            diferencia.TextoGuardado = nombres?.GetValueOrDefault(diferencia.ValorGuardado) ?? diferencia.ValorGuardado;
            diferencia.TextoActual = nombres?.GetValueOrDefault(diferencia.ValorActual) ?? diferencia.ValorActual;
        }

        return new ComparacionDto { Diferencias = diferencias };
    }

    public async Task<NominaDto> Calcular(int idNomina, CalcularDto dto)
    {
        var fechaCalculo = RelojGuatemala.Ahora;

        var nomina = await _nominaRepositorio.ObtenerParaModificar(idNomina)
            ?? throw new ReglaNegocioException("La planilla no existe.", 404);
        if (nomina.Estado == "CERRADA")
        {
            throw new ReglaNegocioException("La planilla está cerrada; no se puede calcular.", 409);
        }

        var actuales = await LeerParametros();
        var datos = await CargarDatosMes(nomina);
        var detalle = await _nominaRepositorio.ObtenerDetalleParaModificar(idNomina);

        var decisiones = dto.Decisiones ?? new List<DecisionDto>();
        ValidarDecisiones(BuscarDiferencias(nomina, detalle, datos.Empleados, actuales), decisiones);

        var parametros = nomina.FechaCalculo == null || decisiones.Any(d => d.IdEmpleado == null && d.Usar == "ACTUAL")
            ? actuales
            : ParametrosGuardados(nomina);

        var filas = detalle.ToDictionary(f => f.IdEmpleado);
        foreach (var idEmpleado in datos.Tramos.Keys)
        {
            var datosEmpleado = DatosEmpleado(idEmpleado, datos, parametros);
            if (datosEmpleado.HorasExtra > 999.99m)
            {
                throw new ReglaNegocioException($"{datos.Empleados[idEmpleado].CodigoEmpleado}: la suma de horas extra del mes ({datosEmpleado.HorasExtra:0.00}) supera 999.99.", 409);
            }

            var camposConValorActual = decisiones
                .Where(d => d.IdEmpleado == idEmpleado && d.Usar == "ACTUAL")
                .Select(d => d.Campo)
                .ToHashSet();

            if (filas.TryGetValue(idEmpleado, out var fila))
            {
                CalculoNomina.ActualizarFila(fila, datosEmpleado, camposConValorActual, false, parametros);
            }
            else
            {
                fila = new NominaDetalle { IdNomina = nomina.IdNomina, IdEmpleado = idEmpleado };
                CalculoNomina.ActualizarFila(fila, datosEmpleado, camposConValorActual, true, parametros);
                _nominaRepositorio.AgregarDetalle(fila);
                filas[idEmpleado] = fila;
            }
        }

        foreach (var fila in detalle.Where(f => !datos.Tramos.ContainsKey(f.IdEmpleado)))
        {
            _nominaRepositorio.EliminarDetalle(fila);
        }

        var vigentes = filas.Values.Where(f => datos.Tramos.ContainsKey(f.IdEmpleado)).ToList();
        nomina.TotalIngresos = vigentes.Sum(f => f.TotalIngresos);
        nomina.TotalDescuentos = vigentes.Sum(f => f.TotalDescuentos);
        nomina.TotalLiquido = vigentes.Sum(f => f.LiquidoRecibir);
        nomina.TotalPatronal = vigentes.Sum(f => f.IgssPatronal + f.Irtra + f.Intecap);
        GuardarParametros(nomina, parametros);
        nomina.Estado = "CALCULADA";
        nomina.FechaCalculo = fechaCalculo;

        await _unidadTrabajo.GuardarCambiosAsync();

        return await ObtenerPorId(idNomina);
    }

    public async Task<NominaDto> Cerrar(int idNomina, CerrarDto dto)
    {
        var nomina = await _nominaRepositorio.ObtenerParaModificar(idNomina)
            ?? throw new ReglaNegocioException("La planilla no existe.", 404);

        byte[] version;
        try
        {
            version = Convert.FromBase64String(dto.Version);
        }
        catch (FormatException)
        {
            throw new ReglaNegocioException("La versión no es válida; recargue la planilla.");
        }
        _nominaRepositorio.FijarVersion(nomina, version);

        if (nomina.Estado == "CERRADA")
        {
            throw new ReglaNegocioException("La planilla ya está cerrada.", 409);
        }
        if (nomina.Estado != "CALCULADA")
        {
            throw new ReglaNegocioException("Solo se puede cerrar una planilla calculada.", 409);
        }
        if (!nomina.Version.SequenceEqual(version))
        {
            throw new ReglaNegocioException("Otra persona modificó la planilla; recárguela y revísela antes de cerrar.", 409, "RECARGAR");
        }

        var datos = await CargarDatosMes(nomina);
        var parametros = ParametrosGuardados(nomina);
        var detalle = await _nominaRepositorio.ObtenerDetalleParaModificar(idNomina);
        var filas = detalle.ToDictionary(f => f.IdEmpleado);

        var hayCambios = datos.Tramos.Keys.Any(id => !filas.ContainsKey(id)) || filas.Keys.Any(id => !datos.Tramos.ContainsKey(id));
        foreach (var fila in detalle)
        {
            if (hayCambios)
            {
                break;
            }

            var antes = CalculoNomina.Firma(fila);
            CalculoNomina.ActualizarFila(fila, DatosEmpleado(fila.IdEmpleado, datos, parametros), new HashSet<string>(), false, parametros);
            hayCambios = CalculoNomina.Firma(fila) != antes;
        }
        if (hayCambios)
        {
            throw new ReglaNegocioException("Hubo cambios desde el último cálculo; recalcule y revise antes de cerrar.", 409, "RECALCULAR");
        }

        var codigos = datos.Empleados.ToDictionary(e => e.Key, e => e.Value.CodigoEmpleado);
        var bloqueos = (await CalcularAdvertencias(nomina, detalle, codigos)).Where(a => a.Bloquea).ToList();
        if (bloqueos.Count > 0)
        {
            var mensajes = string.Join(" ", bloqueos.Take(5).Select(a => a.Mensaje));
            var resto = bloqueos.Count > 5 ? $" Y {bloqueos.Count - 5} más." : "";
            throw new ReglaNegocioException($"No se puede cerrar la planilla. {mensajes}{resto}", 409);
        }

        nomina.Estado = "CERRADA";
        nomina.UsuarioCierre = _usuarioActual.NombreUsuario;
        nomina.FechaCierre = RelojGuatemala.Ahora;
        await _unidadTrabajo.GuardarCambiosAsync();

        return await ObtenerPorId(idNomina);
    }

    public async Task<NominaDto> Reabrir(int idNomina, ReabrirDto dto)
    {
        var nomina = await _nominaRepositorio.ObtenerParaModificar(idNomina)
            ?? throw new ReglaNegocioException("La planilla no existe.", 404);

        if (nomina.Estado != "CERRADA")
        {
            throw new ReglaNegocioException("Solo se puede reabrir una planilla cerrada.", 409);
        }

        nomina.Estado = "CALCULADA";
        nomina.UsuarioCierre = null;
        nomina.FechaCierre = null;
        nomina.MotivoReapertura = dto.Motivo.Trim();
        await _unidadTrabajo.GuardarCambiosAsync();

        return await ObtenerPorId(idNomina);
    }

    private async Task<Nomina> ObtenerExistente(int idNomina)
    {
        return await _nominaRepositorio.ObtenerPorId(idNomina)
            ?? throw new ReglaNegocioException("La planilla no existe.", 404);
    }

    private async Task<DatosMes> CargarDatosMes(Nomina nomina)
    {
        var periodos = await _nominaRepositorio.ObtenerPeriodosEntre(nomina.FechaInicio, nomina.FechaFin);
        var tramos = periodos
            .GroupBy(p => p.IdEmpleado)
            .ToDictionary(g => g.Key, g => CalculoNomina.Tramos(g, nomina.Anio, nomina.Mes));
        var empleados = (await _nominaRepositorio.ObtenerEmpleados(tramos.Keys.ToList()))
            .ToDictionary(e => e.IdEmpleado);
        var asistencias = (await _asistenciaRepositorio.ObtenerTotales(nomina.FechaInicio, nomina.FechaFin))
            .ToDictionary(a => a.IdEmpleado);
        var movimientos = await _nominaRepositorio.ObtenerMovimientos(nomina.IdNomina, null);

        return new DatosMes
        {
            Tramos = tramos,
            Empleados = empleados,
            Asistencias = asistencias,
            Movimientos = movimientos
        };
    }

    private static DatosEmpleadoMes DatosEmpleado(int idEmpleado, DatosMes datos, ParametrosNomina parametros)
    {
        var empleado = datos.Empleados[idEmpleado];
        var tramos = datos.Tramos[idEmpleado];
        datos.Asistencias.TryGetValue(idEmpleado, out var asistencia);
        var movimientos = datos.Movimientos.Where(m => m.IdEmpleado == idEmpleado).ToList();

        var faltas = asistencia?.Faltas ?? 0;
        var suspensiones = asistencia?.SuspensionIgss ?? 0;
        var diasPeriodo = CalculoNomina.DiasPeriodo(tramos, parametros.DiasMes);

        return new DatosEmpleadoMes
        {
            Salario = empleado.Salario,
            RetencionIsr = empleado.RetencionIsr,
            IdPuesto = empleado.IdPuesto,
            IdDepartamento = empleado.IdDepartamento,
            TipoPago = empleado.TipoPago,
            DiasPeriodo = diasPeriodo,
            DiasFalta = faltas,
            DiasSuspension = suspensiones,
            DiasPagar = CalculoNomina.DiasPagar(diasPeriodo, faltas + suspensiones, CalculoNomina.DiasCalendario(tramos)),
            HorasExtra = asistencia?.HorasExtra ?? 0,
            OtrosIngresosAfectos = movimientos
                .Where(m => m.IdRubroNavigation.Tipo == "INGRESO" && m.IdRubroNavigation.AfectoIgss)
                .Sum(m => m.Monto),
            OtrosIngresosNoAfectos = movimientos
                .Where(m => m.IdRubroNavigation.Tipo == "INGRESO" && !m.IdRubroNavigation.AfectoIgss)
                .Sum(m => m.Monto),
            OtrosDescuentos = movimientos
                .Where(m => m.IdRubroNavigation.Tipo == "DESCUENTO")
                .Sum(m => m.Monto)
        };
    }

    private static List<DiferenciaDto> BuscarDiferencias(Nomina nomina, List<NominaDetalle> detalle,
        Dictionary<int, Empleado> empleados, ParametrosNomina actuales)
    {
        var diferencias = new List<DiferenciaDto>();

        foreach (var fila in detalle)
        {
            if (!empleados.TryGetValue(fila.IdEmpleado, out var empleado))
            {
                continue;
            }

            AgregarDiferencia(diferencias, fila.IdEmpleado, CalculoNomina.CampoSalario, Formato(fila.Salario), Formato(empleado.Salario));
            AgregarDiferencia(diferencias, fila.IdEmpleado, CalculoNomina.CampoRetencionIsr, Formato(fila.RetencionIsr), Formato(empleado.RetencionIsr));
            AgregarDiferencia(diferencias, fila.IdEmpleado, CalculoNomina.CampoIdPuesto, fila.IdPuesto.ToString(), empleado.IdPuesto.ToString());
            AgregarDiferencia(diferencias, fila.IdEmpleado, CalculoNomina.CampoIdDepartamento, fila.IdDepartamento.ToString(), empleado.IdDepartamento.ToString());
            AgregarDiferencia(diferencias, fila.IdEmpleado, CalculoNomina.CampoTipoPago, fila.TipoPago, empleado.TipoPago);
        }

        if (nomina.FechaCalculo != null)
        {
            var guardados = ParametrosGuardados(nomina);
            AgregarDiferencia(diferencias, null, ClavesParametro.IgssLaboral, Formato(guardados.IgssLaboral), Formato(actuales.IgssLaboral));
            AgregarDiferencia(diferencias, null, ClavesParametro.IgssPatronal, Formato(guardados.IgssPatronal), Formato(actuales.IgssPatronal));
            AgregarDiferencia(diferencias, null, ClavesParametro.Irtra, Formato(guardados.Irtra), Formato(actuales.Irtra));
            AgregarDiferencia(diferencias, null, ClavesParametro.Intecap, Formato(guardados.Intecap), Formato(actuales.Intecap));
            AgregarDiferencia(diferencias, null, ClavesParametro.BonificacionIncentivo, Formato(guardados.BonificacionIncentivo), Formato(actuales.BonificacionIncentivo));
            AgregarDiferencia(diferencias, null, ClavesParametro.FactorHoraExtra, Formato(guardados.FactorHoraExtra), Formato(actuales.FactorHoraExtra));
            AgregarDiferencia(diferencias, null, ClavesParametro.DiasMes, guardados.DiasMes.ToString(), actuales.DiasMes.ToString());
            AgregarDiferencia(diferencias, null, ClavesParametro.HorasMes, guardados.HorasMes.ToString(), actuales.HorasMes.ToString());
        }

        return diferencias;
    }

    private static void AgregarDiferencia(List<DiferenciaDto> diferencias, int? idEmpleado, string campo, string guardado, string actual)
    {
        if (guardado != actual)
        {
            diferencias.Add(new DiferenciaDto
            {
                IdEmpleado = idEmpleado,
                Campo = campo,
                ValorGuardado = guardado,
                ValorActual = actual
            });
        }
    }

    private static void ValidarDecisiones(List<DiferenciaDto> diferencias, List<DecisionDto> decisiones)
    {
        if (decisiones.Any(d => d == null))
        {
            throw new ReglaNegocioException("Hay decisiones vacías en la lista.");
        }
        if (decisiones.Where(d => d.IdEmpleado == null).Select(d => d.Usar).Distinct().Count() > 1)
        {
            throw new ReglaNegocioException("Los parámetros se deciden juntos: use la misma opción para todos.");
        }

        var completas = decisiones.Count == diferencias.Count && diferencias.All(dif =>
        {
            var coincidentes = decisiones.Where(d => d.IdEmpleado == dif.IdEmpleado && d.Campo == dif.Campo).ToList();
            return coincidentes.Count == 1 && coincidentes[0].ValorActual == dif.ValorActual;
        });
        if (!completas)
        {
            throw new ReglaNegocioException("Los datos cambiaron desde la comparación; vuelva a comparar antes de calcular.", 409, "COMPARAR_DE_NUEVO");
        }
    }

    private async Task<List<AdvertenciaDto>> CalcularAdvertencias(Nomina nomina, List<NominaDetalle> detalle,
        Dictionary<int, string> codigos)
    {
        var advertencias = new List<AdvertenciaDto>();
        if (nomina.Estado == "CERRADA")
        {
            return advertencias;
        }

        foreach (var fila in detalle)
        {
            advertencias.AddRange(AdvertenciasFila(fila, nomina.DiasMes, codigos.GetValueOrDefault(fila.IdEmpleado)));
        }

        var hoy = Hoy();
        if (nomina.FechaInicio > hoy)
        {
            advertencias.Add(Advertencia("MES_NO_INICIADO", "El mes de la planilla todavía no inicia.", true, null));
        }
        else if (nomina.FechaFin > hoy)
        {
            advertencias.Add(Advertencia("MES_NO_TERMINADO", "El mes todavía no termina; puede haber asistencias pendientes.", false, null));
        }

        var masAntigua = await _nominaRepositorio.ObtenerInicioMasAntiguo();
        var anterior = nomina.FechaInicio.AddMonths(-1);
        if (masAntigua < nomina.FechaInicio && !await _nominaRepositorio.Existe(anterior.Year, anterior.Month))
        {
            advertencias.Add(Advertencia("MES_ANTERIOR_FALTANTE", $"Falta crear la planilla de {anterior:MM'/'yyyy}.", true, null));
        }

        var movimientos = await _nominaRepositorio.ObtenerMovimientos(nomina.IdNomina, null);
        var conPeriodo = (await _nominaRepositorio.ObtenerPeriodosEntre(nomina.FechaInicio, nomina.FechaFin))
            .Select(p => p.IdEmpleado)
            .ToHashSet();
        var sinPeriodo = movimientos
            .Where(m => !conPeriodo.Contains(m.IdEmpleado))
            .Select(m => m.IdEmpleadoNavigation.CodigoEmpleado)
            .Distinct()
            .ToList();
        if (sinPeriodo.Count > 0)
        {
            advertencias.Add(Advertencia("MOVIMIENTOS_SIN_PERIODO",
                $"Hay movimientos de empleados sin periodo laboral en el mes: {string.Join(", ", sinPeriodo)}. Elimínelos o corrija el periodo.", true, null));
        }

        return advertencias;
    }

    private static List<AdvertenciaDto> AdvertenciasFila(NominaDetalle fila, int? diasMes, string codigo)
    {
        var advertencias = new List<AdvertenciaDto>();

        if (fila.LiquidoRecibir < 0)
        {
            advertencias.Add(Advertencia("LIQUIDO_NEGATIVO",
                $"{codigo}: el líquido a recibir es negativo (Q{Monto(fila.LiquidoRecibir)}).", true, fila.IdEmpleado));
        }
        if (diasMes.HasValue && fila.DiasPeriodo < diasMes.Value && fila.Isr > 0)
        {
            advertencias.Add(Advertencia("ISR_PERIODO_PARCIAL",
                $"{codigo}: trabajó parte del mes y se le retiene el ISR completo (Q{Monto(fila.Isr)}); revise si corresponde.", false, fila.IdEmpleado));
        }

        return advertencias;
    }

    private static AdvertenciaDto Advertencia(string codigo, string mensaje, bool bloquea, int? idEmpleado)
    {
        return new AdvertenciaDto
        {
            Codigo = codigo,
            Mensaje = mensaje,
            Bloquea = bloquea,
            IdEmpleado = idEmpleado
        };
    }

    private async Task<ParametrosNomina> LeerParametros()
    {
        var valores = (await _parametroRepositorio.ObtenerTodos()).ToDictionary(p => p.Clave, p => p.Valor);

        return new ParametrosNomina
        {
            IgssLaboral = LeerDecimal(valores, ClavesParametro.IgssLaboral, 0, 100, 4),
            IgssPatronal = LeerDecimal(valores, ClavesParametro.IgssPatronal, 0, 100, 4),
            Irtra = LeerDecimal(valores, ClavesParametro.Irtra, 0, 100, 4),
            Intecap = LeerDecimal(valores, ClavesParametro.Intecap, 0, 100, 4),
            BonificacionIncentivo = LeerDecimal(valores, ClavesParametro.BonificacionIncentivo, 0, 99999999.99m, 2),
            FactorHoraExtra = LeerDecimal(valores, ClavesParametro.FactorHoraExtra, 1, 10, 4),
            DiasMes = (int)LeerDecimal(valores, ClavesParametro.DiasMes, 1, 31, 0),
            HorasMes = (int)LeerDecimal(valores, ClavesParametro.HorasMes, 1, 999, 0)
        };
    }

    private static decimal LeerDecimal(Dictionary<string, string> valores, string clave, decimal minimo, decimal maximo,
        int decimales)
    {
        if (!valores.TryGetValue(clave, out var texto)
            || !decimal.TryParse(texto, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var valor)
            || valor < minimo || valor > maximo || valor != Math.Round(valor, decimales))
        {
            throw new InvalidOperationException($"El parámetro {clave} no existe o no es válido.");
        }

        return valor;
    }

    private static ParametrosNomina ParametrosGuardados(Nomina nomina)
    {
        return new ParametrosNomina
        {
            IgssLaboral = nomina.IgssLaboral.Value,
            IgssPatronal = nomina.IgssPatronal.Value,
            Irtra = nomina.Irtra.Value,
            Intecap = nomina.Intecap.Value,
            BonificacionIncentivo = nomina.BonificacionIncentivo.Value,
            FactorHoraExtra = nomina.FactorHoraExtra.Value,
            DiasMes = nomina.DiasMes.Value,
            HorasMes = nomina.HorasMes.Value
        };
    }

    private static void GuardarParametros(Nomina nomina, ParametrosNomina parametros)
    {
        nomina.IgssLaboral = parametros.IgssLaboral;
        nomina.IgssPatronal = parametros.IgssPatronal;
        nomina.Irtra = parametros.Irtra;
        nomina.Intecap = parametros.Intecap;
        nomina.BonificacionIncentivo = parametros.BonificacionIncentivo;
        nomina.FactorHoraExtra = parametros.FactorHoraExtra;
        nomina.DiasMes = parametros.DiasMes;
        nomina.HorasMes = parametros.HorasMes;
    }

    private static string Formato(decimal valor)
    {
        return valor.ToString("0.####", CultureInfo.InvariantCulture);
    }

    private static string Monto(decimal valor)
    {
        return valor.ToString("#,##0.00", CultureInfo.InvariantCulture);
    }

    private static DateOnly Hoy()
    {
        return RelojGuatemala.Hoy;
    }

    private static NominaDto ADto(Nomina nomina, int cantidadEmpleados, List<AdvertenciaDto> advertencias)
    {
        return new NominaDto
        {
            IdNomina = nomina.IdNomina,
            Anio = nomina.Anio,
            Mes = nomina.Mes,
            Estado = nomina.Estado,
            FechaInicio = nomina.FechaInicio,
            FechaFin = nomina.FechaFin,
            TotalIngresos = nomina.TotalIngresos,
            TotalDescuentos = nomina.TotalDescuentos,
            TotalLiquido = nomina.TotalLiquido,
            TotalPatronal = nomina.TotalPatronal,
            CantidadEmpleados = cantidadEmpleados,
            FechaCalculo = nomina.FechaCalculo,
            IgssLaboral = nomina.IgssLaboral,
            IgssPatronal = nomina.IgssPatronal,
            Irtra = nomina.Irtra,
            Intecap = nomina.Intecap,
            BonificacionIncentivo = nomina.BonificacionIncentivo,
            FactorHoraExtra = nomina.FactorHoraExtra,
            DiasMes = nomina.DiasMes,
            HorasMes = nomina.HorasMes,
            UsuarioCierre = nomina.UsuarioCierre,
            FechaCierre = nomina.FechaCierre,
            MotivoReapertura = nomina.MotivoReapertura,
            Version = nomina.Version == null ? null : Convert.ToBase64String(nomina.Version),
            PuedeCerrar = advertencias == null ? null : nomina.Estado == "CALCULADA" && !advertencias.Any(a => a.Bloquea),
            Advertencias = advertencias
        };
    }

    private static NominaDetalleDto ADetalleDto(NominaDetalle fila, List<AdvertenciaDto> advertencias)
    {
        return new NominaDetalleDto
        {
            IdNominaDetalle = fila.IdNominaDetalle,
            IdEmpleado = fila.IdEmpleado,
            CodigoEmpleado = fila.IdEmpleadoNavigation.CodigoEmpleado,
            NombreCompleto = $"{fila.IdEmpleadoNavigation.Nombres} {fila.IdEmpleadoNavigation.Apellidos}",
            IdPuesto = fila.IdPuesto,
            NombrePuesto = fila.IdPuestoNavigation.Nombre,
            IdDepartamento = fila.IdDepartamento,
            NombreDepartamento = fila.IdDepartamentoNavigation.Nombre,
            Salario = fila.Salario,
            TipoPago = fila.TipoPago,
            RetencionIsr = fila.RetencionIsr,
            Isr = fila.Isr,
            DiasPeriodo = fila.DiasPeriodo,
            DiasFalta = fila.DiasFalta,
            DiasSuspension = fila.DiasSuspension,
            DiasPagar = fila.DiasPagar,
            HorasExtra = fila.HorasExtra,
            SalarioDevengado = fila.SalarioDevengado,
            MontoHorasExtra = fila.MontoHorasExtra,
            Bonificacion = fila.Bonificacion,
            OtrosIngresosAfectos = fila.OtrosIngresosAfectos,
            OtrosIngresosNoAfectos = fila.OtrosIngresosNoAfectos,
            TotalIngresos = fila.TotalIngresos,
            IgssLaboral = fila.IgssLaboral,
            OtrosDescuentos = fila.OtrosDescuentos,
            TotalDescuentos = fila.TotalDescuentos,
            LiquidoRecibir = fila.LiquidoRecibir,
            IgssPatronal = fila.IgssPatronal,
            Irtra = fila.Irtra,
            Intecap = fila.Intecap,
            Advertencias = advertencias
        };
    }

    private sealed class DatosMes
    {
        public Dictionary<int, List<(DateOnly Desde, DateOnly Hasta)>> Tramos { get; init; }
        public Dictionary<int, Empleado> Empleados { get; init; }
        public Dictionary<int, ResumenAsistenciaDto> Asistencias { get; init; }
        public List<MovimientoNomina> Movimientos { get; init; }
    }
}
