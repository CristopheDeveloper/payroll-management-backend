using System.Globalization;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Excepciones;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Calculo;
using Planilla.Dominio.Constantes;
using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Servicios;

public class NominaServicio : INominaServicio
{
    private readonly INominaRepositorio _nominaRepositorio;
    private readonly IAsistenciaRepositorio _asistenciaRepositorio;
    private readonly IParametroRepositorio _parametroRepositorio;
    private readonly IRubroRepositorio _rubroRepositorio;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IUnidadTrabajo _unidadTrabajo;

    public NominaServicio(INominaRepositorio nominaRepositorio, IAsistenciaRepositorio asistenciaRepositorio,
        IParametroRepositorio parametroRepositorio, IRubroRepositorio rubroRepositorio, IUsuarioActual usuarioActual,
        IUnidadTrabajo unidadTrabajo)
    {
        _nominaRepositorio = nominaRepositorio;
        _asistenciaRepositorio = asistenciaRepositorio;
        _parametroRepositorio = parametroRepositorio;
        _rubroRepositorio = rubroRepositorio;
        _usuarioActual = usuarioActual;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<List<TipoPlanillaDto>> ObtenerTiposPlanilla(bool soloActivos)
    {
        var tipos = await _nominaRepositorio.ObtenerTiposPlanilla(soloActivos);

        return tipos.Select(t => new TipoPlanillaDto
        {
            IdTipoPlanilla = t.IdTipoPlanilla,
            Codigo = t.Codigo,
            Nombre = t.Nombre,
            Descripcion = t.Descripcion,
            IncluyeIgss = t.IncluyeIgss,
            Activo = t.Activo
        }).ToList();
    }

    public async Task<List<NominaDto>> ObtenerTodas(int? anio, int? idTipoPlanilla)
    {
        var nominas = await _nominaRepositorio.ObtenerTodas(anio, idTipoPlanilla);
        var cantidades = await _nominaRepositorio.ContarEmpleados(nominas.Select(n => n.IdNomina).ToList());

        return nominas.Select(n => ADto(n, cantidades.GetValueOrDefault(n.IdNomina), null)).ToList();
    }

    public async Task<NominaDto> ObtenerPorId(int idNomina)
    {
        var nomina = await ObtenerExistente(idNomina);
        var empleados = await _nominaRepositorio.ObtenerEmpleadosNomina(idNomina);
        var advertencias = await CalcularAdvertencias(nomina, empleados);

        return ADto(nomina, empleados.Count, advertencias);
    }

    public async Task<NominaDto> Crear(NominaCrearDto dto)
    {
        var tipo = await _nominaRepositorio.ObtenerTipoPlanilla(dto.IdTipoPlanilla)
            ?? throw new ReglaNegocioException("El tipo de planilla no existe.", 404);
        if (!tipo.Activo)
        {
            throw new ReglaNegocioException($"El tipo de planilla '{tipo.Nombre}' no está habilitado.", 409);
        }

        var inicio = new DateOnly(dto.Anio, dto.Mes, 1);
        var fin = inicio.AddMonths(1).AddDays(-1);
        var hoy = Hoy();
        if (inicio > new DateOnly(hoy.Year, hoy.Month, 1).AddMonths(1))
        {
            throw new ReglaNegocioException("Solo se pueden crear planillas hasta el mes siguiente.");
        }
        var masAntigua = await _nominaRepositorio.ObtenerInicioMasAntiguo(tipo.IdTipoPlanilla);
        if (masAntigua.HasValue && inicio < masAntigua.Value)
        {
            throw new ReglaNegocioException($"No se pueden crear planillas anteriores a la más antigua ({masAntigua.Value:MM'/'yyyy}).", 409);
        }

        var codigo = string.IsNullOrWhiteSpace(dto.Codigo) ? await CodigoLibre($"MEN-{dto.Anio}-{dto.Mes:00}") : dto.Codigo.Trim().ToUpper();
        if (await _nominaRepositorio.ExisteCodigo(codigo, null))
        {
            throw new ReglaNegocioException($"Ya existe una planilla con el código '{codigo}'.", 409);
        }

        var fechaPago = dto.FechaPago ?? fin;
        ValidarFechaPago(fechaPago, inicio);

        var nomina = new Nomina
        {
            IdTipoPlanilla = tipo.IdTipoPlanilla,
            Codigo = codigo,
            Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion)
                ? $"Salario correspondiente al mes de {NombreMes(dto.Mes)} {dto.Anio}"
                : dto.Descripcion.Trim(),
            Anio = dto.Anio,
            Mes = dto.Mes,
            FechaInicio = inicio,
            FechaFin = fin,
            FechaPago = fechaPago,
            Comentario = Texto(dto.Comentario),
            Estado = "CREADA"
        };

        _nominaRepositorio.Agregar(nomina);
        await _unidadTrabajo.GuardarCambiosAsync();

        return await ObtenerPorId(nomina.IdNomina);
    }

    private async Task<string> CodigoLibre(string baseCodigo)
    {
        var codigo = baseCodigo;
        for (var numero = 2; await _nominaRepositorio.ExisteCodigo(codigo, null); numero++)
        {
            codigo = $"{baseCodigo}-{numero}";
        }
        return codigo;
    }

    public async Task<NominaDto> Editar(int idNomina, NominaEditarDto dto)
    {
        var nomina = await ObtenerParaModificar(idNomina);
        if (nomina.Estado == "CERRADA")
        {
            throw new ReglaNegocioException("Reabra la planilla antes de editar el encabezado.", 409);
        }

        var codigo = dto.Codigo.Trim().ToUpper();
        if (codigo != nomina.Codigo && nomina.Estado != "CREADA")
        {
            throw new ReglaNegocioException("El código solo se puede cambiar en una planilla recién creada.", 409);
        }
        if (await _nominaRepositorio.ExisteCodigo(codigo, idNomina))
        {
            throw new ReglaNegocioException($"Ya existe una planilla con el código '{codigo}'.", 409);
        }
        ValidarFechaPago(dto.FechaPago.Value, nomina.FechaInicio);

        nomina.Codigo = codigo;
        nomina.Descripcion = dto.Descripcion.Trim();
        nomina.FechaPago = dto.FechaPago.Value;
        nomina.Comentario = Texto(dto.Comentario);
        await _unidadTrabajo.GuardarCambiosAsync();

        return await ObtenerPorId(idNomina);
    }

    public async Task Eliminar(int idNomina)
    {
        var nomina = await ObtenerParaModificar(idNomina);
        if (nomina.Estado == "CERRADA")
        {
            throw new ReglaNegocioException("Reabra la planilla antes de eliminarla.", 409);
        }

        foreach (var fila in await _nominaRepositorio.ObtenerEmpleadosNominaParaModificar(idNomina))
        {
            EliminarFila(fila);
        }
        _nominaRepositorio.Eliminar(nomina);

        await _unidadTrabajo.GuardarCambiosAsync();
    }

    public async Task<NominaDto> Preparar(int idNomina)
    {
        var nomina = await ObtenerParaModificar(idNomina);
        if (nomina.Estado != "CREADA" && nomina.Estado != "PREPARADA")
        {
            throw new ReglaNegocioException(nomina.Estado == "GENERADA"
                ? "La planilla ya está generada; vuelva a preparada antes de preparar de nuevo."
                : "La planilla está cerrada; no se puede preparar.", 409);
        }

        var parametros = await LeerParametros();
        var periodos = await _nominaRepositorio.ObtenerPeriodosEntre(nomina.FechaInicio, nomina.FechaFin);
        var tramos = periodos
            .GroupBy(p => p.IdEmpleado)
            .ToDictionary(g => g.Key, g => CalculoNomina.Tramos(g, nomina.Anio, nomina.Mes));
        if (tramos.Count == 0)
        {
            throw new ReglaNegocioException("No hay empleados con periodo laboral en el periodo de la planilla.");
        }

        var empleados = (await _nominaRepositorio.ObtenerEmpleados(tramos.Keys.ToList())).ToDictionary(e => e.IdEmpleado);
        var asistencias = (await _asistenciaRepositorio.ObtenerTotales(nomina.FechaInicio, nomina.FechaFin))
            .ToDictionary(a => a.IdEmpleado);
        var filas = (await _nominaRepositorio.ObtenerEmpleadosNominaParaModificar(idNomina)).ToDictionary(f => f.IdEmpleado);

        foreach (var (idEmpleado, tramosEmpleado) in tramos)
        {
            var empleado = empleados[idEmpleado];
            asistencias.TryGetValue(idEmpleado, out var asistencia);
            var faltas = asistencia?.Faltas ?? 0;
            var suspension = asistencia?.SuspensionIgss ?? 0;

            var diasPeriodo = CalculoNomina.DiasPeriodo(tramosEmpleado, parametros.DiasMes);
            var diasSugeridos = CalculoNomina.DiasPagar(diasPeriodo, faltas + suspension, CalculoNomina.DiasCalendario(tramosEmpleado));

            if (!filas.TryGetValue(idEmpleado, out var fila))
            {
                fila = new NominaEmpleado { IdNomina = idNomina, IdEmpleado = idEmpleado };
                _nominaRepositorio.AgregarEmpleado(fila);
            }

            var nueva = fila.IdNominaEmpleado == 0;
            var conservarDias = !nueva && fila.DatosAjustados;
            var isrDeFicha = nueva || fila.Isr == fila.RetencionIsr;
            fila.IdPuesto = empleado.IdPuesto;
            fila.IdDepartamento = empleado.IdDepartamento;
            fila.Salario = empleado.Salario;
            fila.RetencionIsr = empleado.RetencionIsr;
            fila.TipoPago = empleado.TipoPago;
            fila.Banco = empleado.Banco;
            fila.CuentaBancaria = empleado.CuentaBancaria;
            fila.DiasPeriodo = diasPeriodo;
            fila.DiasFalta = faltas;
            fila.DiasSuspension = suspension;
            fila.DiasPagarSugeridos = diasSugeridos;
            fila.HorasExtraSugeridas = 0;
            fila.DiasPagar = conservarDias ? Math.Min(fila.DiasPagar, diasPeriodo) : diasSugeridos;
            if (isrDeFicha)
            {
                fila.Isr = empleado.RetencionIsr;
            }
            fila.DatosAjustados = EstaAjustada(fila);
            LimpiarTotales(fila);
        }

        foreach (var fila in filas.Values.Where(f => !tramos.ContainsKey(f.IdEmpleado)))
        {
            EliminarFila(fila);
        }

        GuardarParametros(nomina, parametros);
        nomina.Estado = "PREPARADA";
        nomina.FechaPreparacion = DateTime.Now;
        nomina.FechaGeneracion = null;
        LimpiarTotales(nomina);
        await _unidadTrabajo.GuardarCambiosAsync();

        return await ObtenerPorId(idNomina);
    }

    public async Task<List<NominaEmpleadoDto>> ObtenerEmpleados(int idNomina)
    {
        var nomina = await ObtenerExistente(idNomina);
        var empleados = await _nominaRepositorio.ObtenerEmpleadosNomina(idNomina);
        var conPeriodo = await EmpleadosConPeriodo(nomina);

        return empleados.Select(e => AEmpleadoDto(e, AdvertenciasFila(nomina, e, conPeriodo))).ToList();
    }

    public async Task<NominaEmpleadoDto> GuardarDatosEmpleado(int idNomina, int idEmpleado, DatosEmpleadoGuardarDto dto)
    {
        await ObtenerPreparada(idNomina);
        var filas = await _nominaRepositorio.ObtenerEmpleadosNominaParaModificar(idNomina);
        var fila = filas.FirstOrDefault(f => f.IdEmpleado == idEmpleado)
            ?? throw new ReglaNegocioException("El empleado no está en la planilla.", 404);

        if (dto.Valores.Any(v => v == null))
        {
            throw new ReglaNegocioException("Hay valores vacíos en la lista.");
        }
        if (dto.Valores.GroupBy(v => v.IdRubro).Any(g => g.Count() > 1))
        {
            throw new ReglaNegocioException("Hay rubros repetidos en la lista.");
        }

        var rubros = await RubrosManuales();
        AsignarDias(fila, dto.DiasPagar);
        AsignarHoras(fila, dto.HorasExtra);
        AsignarIsr(fila, dto.Isr);

        foreach (var valor in fila.NominaValor.Where(v => dto.Valores.All(d => d.IdRubro != v.IdRubro || d.Monto == 0)).ToList())
        {
            fila.NominaValor.Remove(valor);
            _nominaRepositorio.EliminarValor(valor);
        }
        foreach (var valor in dto.Valores.Where(v => v.Monto > 0))
        {
            AsignarValor(fila, rubros, valor.IdRubro, valor.Monto, valor.Descripcion);
        }

        fila.DatosAjustados = EstaAjustada(fila);
        await _unidadTrabajo.GuardarCambiosAsync();

        return (await ObtenerEmpleados(idNomina)).First(e => e.IdEmpleado == idEmpleado);
    }

    public async Task<List<NominaEmpleadoDto>> GuardarVariable(int idNomina, VariableGuardarDto dto)
    {
        return await GuardarVariables(idNomina, new ImportarVariablesDto { Variables = [dto] });
    }

    public async Task<List<NominaEmpleadoDto>> GuardarVariables(int idNomina, ImportarVariablesDto dto)
    {
        await ObtenerPreparada(idNomina);
        if (dto.Variables.Any(v => v == null))
        {
            throw new ReglaNegocioException("Hay variables vacías en la lista.");
        }
        if (dto.Variables.GroupBy(v => (v.Variable, v.IdRubro)).Any(g => g.Count() > 1))
        {
            throw new ReglaNegocioException("Hay variables repetidas en la lista.");
        }
        foreach (var variable in dto.Variables)
        {
            ValidarVariable(variable);
        }

        var filas = (await _nominaRepositorio.ObtenerEmpleadosNominaParaModificar(idNomina)).ToDictionary(f => f.IdEmpleado);
        var fuera = dto.Variables.SelectMany(v => v.Valores).Where(v => !filas.ContainsKey(v.IdEmpleado)).Select(v => v.IdEmpleado).Distinct().ToList();
        if (fuera.Count > 0)
        {
            throw new ReglaNegocioException($"Estos empleados no están en la planilla (Id): {string.Join(", ", fuera)}.", 404);
        }

        var rubros = dto.Variables.Any(v => v.Variable == "RUBRO") ? await RubrosManuales() : null;
        foreach (var variable in dto.Variables)
        {
            AplicarVariable(filas, rubros, variable);
        }

        await _unidadTrabajo.GuardarCambiosAsync();

        var ids = dto.Variables.SelectMany(v => v.Valores).Select(v => v.IdEmpleado).ToHashSet();
        return (await ObtenerEmpleados(idNomina)).Where(e => ids.Contains(e.IdEmpleado)).ToList();
    }

    private static void ValidarVariable(VariableGuardarDto dto)
    {
        if (dto.Valores.Any(v => v == null))
        {
            throw new ReglaNegocioException("Hay valores vacíos en la lista.");
        }
        if (dto.Valores.GroupBy(v => v.IdEmpleado).Any(g => g.Count() > 1))
        {
            throw new ReglaNegocioException("Hay empleados repetidos en la lista.");
        }
        if (dto.Variable == "RUBRO" && dto.IdRubro == null)
        {
            throw new ReglaNegocioException("Indique el rubro de la variable.");
        }
        if (dto.Variable != "RUBRO" && dto.IdRubro != null)
        {
            throw new ReglaNegocioException("El rubro solo se indica cuando la variable es RUBRO.");
        }
    }

    private void AplicarVariable(Dictionary<int, NominaEmpleado> filas, Dictionary<int, Rubro> rubros, VariableGuardarDto dto)
    {
        foreach (var valor in dto.Valores)
        {
            var fila = filas[valor.IdEmpleado];
            switch (dto.Variable)
            {
                case "DIAS_PAGAR":
                    if (valor.Valor != Math.Truncate(valor.Valor))
                    {
                        throw new ReglaNegocioException($"{fila.IdEmpleadoNavigation.CodigoEmpleado}: los días a pagar deben ser un número entero.");
                    }
                    AsignarDias(fila, (int)valor.Valor);
                    break;
                case "HORAS_EXTRA":
                    AsignarHoras(fila, valor.Valor);
                    break;
                case "ISR":
                    AsignarIsr(fila, valor.Valor);
                    break;
                default:
                    var existente = fila.NominaValor.FirstOrDefault(v => v.IdRubro == dto.IdRubro);
                    if (valor.Valor == 0)
                    {
                        if (existente != null)
                        {
                            fila.NominaValor.Remove(existente);
                            _nominaRepositorio.EliminarValor(existente);
                        }
                    }
                    else
                    {
                        AsignarValor(fila, rubros, dto.IdRubro.Value, valor.Valor, valor.Descripcion);
                    }
                    break;
            }
            fila.DatosAjustados = EstaAjustada(fila);
        }
    }

    public async Task<NominaDto> Generar(int idNomina)
    {
        var nomina = await ObtenerParaModificar(idNomina);
        if (nomina.Estado != "PREPARADA" && nomina.Estado != "GENERADA")
        {
            throw new ReglaNegocioException(nomina.Estado == "CREADA"
                ? "Prepare la planilla antes de generarla."
                : "La planilla está cerrada; no se puede generar.", 409);
        }

        var parametros = ParametrosGuardados(nomina);
        var sistema = (await _rubroRepositorio.ObtenerTodos(false, null, "SISTEMA")).ToDictionary(r => r.Codigo);
        var manuales = (await _rubroRepositorio.ObtenerTodos(false, null, "MANUAL")).ToDictionary(r => r.IdRubro);
        var filas = await _nominaRepositorio.ObtenerEmpleadosNominaParaModificar(idNomina);

        foreach (var fila in filas)
        {
            foreach (var anterior in fila.NominaRubro.ToList())
            {
                fila.NominaRubro.Remove(anterior);
                _nominaRepositorio.EliminarRubro(anterior);
            }

            var datos = new DatosEmpleadoMes
            {
                Salario = fila.Salario,
                Isr = fila.Isr,
                DiasPagar = fila.DiasPagar,
                HorasExtra = fila.HorasExtra,
                Valores = fila.NominaValor.Select(v => new ValorManual
                {
                    IdRubro = v.IdRubro,
                    Tipo = manuales[v.IdRubro].Tipo,
                    AfectoIgss = manuales[v.IdRubro].AfectoIgss,
                    Orden = manuales[v.IdRubro].Orden,
                    Monto = v.Monto,
                    Descripcion = v.Descripcion
                }).ToList()
            };

            var calculados = CalculoNomina.GenerarRubros(datos, parametros);
            foreach (var calculado in calculados)
            {
                var rubro = calculado.IdRubro.HasValue
                    ? manuales[calculado.IdRubro.Value]
                    : sistema.GetValueOrDefault(calculado.Codigo)
                        ?? throw new InvalidOperationException($"Falta el rubro del sistema {calculado.Codigo}.");

                var nuevo = new NominaRubro
                {
                    IdNominaEmpleado = fila.IdNominaEmpleado,
                    IdRubro = rubro.IdRubro,
                    Orden = rubro.Orden,
                    Base = calculado.Base,
                    Monto = calculado.Monto,
                    Descripcion = calculado.Descripcion
                };
                fila.NominaRubro.Add(nuevo);
                _nominaRepositorio.AgregarRubro(nuevo);
            }

            fila.TotalIngresos = CalculoNomina.Total(calculados, "INGRESO");
            fila.TotalDescuentos = CalculoNomina.Total(calculados, "DESCUENTO");
            fila.Liquido = fila.TotalIngresos - fila.TotalDescuentos;
            fila.TotalPatronal = CalculoNomina.Total(calculados, "PATRONAL");
        }

        nomina.TotalIngresos = filas.Sum(f => f.TotalIngresos);
        nomina.TotalDescuentos = filas.Sum(f => f.TotalDescuentos);
        nomina.TotalLiquido = filas.Sum(f => f.Liquido);
        nomina.TotalPatronal = filas.Sum(f => f.TotalPatronal);
        nomina.Estado = "GENERADA";
        nomina.FechaGeneracion = DateTime.Now;
        await _unidadTrabajo.GuardarCambiosAsync();

        return await ObtenerPorId(idNomina);
    }

    public async Task<NominaDto> VolverAPreparada(int idNomina)
    {
        var nomina = await ObtenerParaModificar(idNomina);
        if (nomina.Estado != "GENERADA")
        {
            throw new ReglaNegocioException("Solo una planilla generada puede volver a preparada.", 409);
        }

        foreach (var fila in await _nominaRepositorio.ObtenerEmpleadosNominaParaModificar(idNomina))
        {
            foreach (var rubro in fila.NominaRubro.ToList())
            {
                fila.NominaRubro.Remove(rubro);
                _nominaRepositorio.EliminarRubro(rubro);
            }
            LimpiarTotales(fila);
        }

        nomina.Estado = "PREPARADA";
        nomina.FechaGeneracion = null;
        LimpiarTotales(nomina);
        await _unidadTrabajo.GuardarCambiosAsync();

        return await ObtenerPorId(idNomina);
    }

    public async Task<List<ResumenRubroDto>> ObtenerResumenRubros(int idNomina)
    {
        await ObtenerExistente(idNomina);
        var empleados = await _nominaRepositorio.ObtenerEmpleadosNomina(idNomina);

        return empleados
            .SelectMany(e => e.NominaRubro)
            .GroupBy(r => r.IdRubro)
            .Select(g => new ResumenRubroDto
            {
                IdRubro = g.Key,
                Codigo = g.First().IdRubroNavigation.Codigo,
                Nombre = g.First().IdRubroNavigation.Nombre,
                Tipo = g.First().IdRubroNavigation.Tipo,
                Orden = g.First().Orden,
                Empleados = g.Count(),
                Total = g.Sum(r => r.Monto)
            })
            .OrderBy(r => r.Orden)
            .ToList();
    }

    public async Task<NominaDto> Cerrar(int idNomina, CerrarDto dto)
    {
        var nomina = await ObtenerParaModificar(idNomina);

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
        if (nomina.Estado != "GENERADA")
        {
            throw new ReglaNegocioException("Solo se puede cerrar una planilla generada.", 409);
        }
        if (!nomina.Version.SequenceEqual(version))
        {
            throw new ReglaNegocioException("Otra persona modificó la planilla; recárguela y revísela antes de cerrar.", 409, "RECARGAR");
        }

        var empleados = await _nominaRepositorio.ObtenerEmpleadosNomina(idNomina);
        var bloqueos = (await CalcularAdvertencias(nomina, empleados)).Where(a => a.Bloquea).ToList();
        if (bloqueos.Count > 0)
        {
            var mensajes = string.Join(" ", bloqueos.Take(5).Select(a => a.Mensaje));
            var resto = bloqueos.Count > 5 ? $" Y {bloqueos.Count - 5} más." : "";
            throw new ReglaNegocioException($"No se puede cerrar la planilla. {mensajes}{resto}", 409);
        }

        nomina.Estado = "CERRADA";
        nomina.UsuarioCierre = _usuarioActual.NombreUsuario;
        nomina.FechaCierre = DateTime.Now;
        await _unidadTrabajo.GuardarCambiosAsync();

        return await ObtenerPorId(idNomina);
    }

    public async Task<NominaDto> Reabrir(int idNomina, ReabrirDto dto)
    {
        var nomina = await ObtenerParaModificar(idNomina);
        if (nomina.Estado != "CERRADA")
        {
            throw new ReglaNegocioException("Solo se puede reabrir una planilla cerrada.", 409);
        }

        nomina.Estado = "GENERADA";
        nomina.UsuarioCierre = null;
        nomina.FechaCierre = null;
        nomina.MotivoReapertura = dto.Motivo.Trim();
        await _unidadTrabajo.GuardarCambiosAsync();

        return await ObtenerPorId(idNomina);
    }

    public async Task<List<PagoEmpleadoDto>> ObtenerPagosEmpleado(int idEmpleado)
    {
        var pagos = await _nominaRepositorio.ObtenerPagosEmpleado(idEmpleado);

        return pagos.Select(p => new PagoEmpleadoDto
        {
            IdNomina = p.IdNomina,
            Codigo = p.IdNominaNavigation.Codigo,
            Descripcion = p.IdNominaNavigation.Descripcion,
            NombreTipoPlanilla = p.IdNominaNavigation.IdTipoPlanillaNavigation.Nombre,
            Anio = p.IdNominaNavigation.Anio,
            Mes = p.IdNominaNavigation.Mes,
            FechaPago = p.IdNominaNavigation.FechaPago,
            DiasPagar = p.DiasPagar,
            TotalIngresos = p.TotalIngresos,
            TotalDescuentos = p.TotalDescuentos,
            Liquido = p.Liquido,
            Rubros = p.NominaRubro.OrderBy(r => r.Orden).Select(ARubroDto).ToList()
        }).ToList();
    }

    private async Task<Nomina> ObtenerExistente(int idNomina)
    {
        return await _nominaRepositorio.ObtenerPorId(idNomina)
            ?? throw new ReglaNegocioException("La planilla no existe.", 404);
    }

    private async Task<Nomina> ObtenerParaModificar(int idNomina)
    {
        return await _nominaRepositorio.ObtenerParaModificar(idNomina)
            ?? throw new ReglaNegocioException("La planilla no existe.", 404);
    }

    private async Task<Nomina> ObtenerPreparada(int idNomina)
    {
        var nomina = await ObtenerExistente(idNomina);
        if (nomina.Estado != "PREPARADA")
        {
            throw new ReglaNegocioException(nomina.Estado switch
            {
                "CREADA" => "Prepare la planilla antes de capturar datos.",
                "GENERADA" => "La planilla ya está generada; vuelva a preparada para cambiar datos.",
                _ => "La planilla está cerrada; no se pueden cambiar datos."
            }, 409);
        }

        return nomina;
    }

    private async Task<Dictionary<int, Rubro>> RubrosManuales()
    {
        return (await _rubroRepositorio.ObtenerTodos(false, null, "MANUAL")).ToDictionary(r => r.IdRubro);
    }

    private void AsignarValor(NominaEmpleado fila, Dictionary<int, Rubro> rubros, int idRubro, decimal monto, string descripcion)
    {
        if (!rubros.TryGetValue(idRubro, out var rubro))
        {
            throw new ReglaNegocioException($"El rubro {idRubro} no existe o no es un rubro manual.", 404);
        }

        var existente = fila.NominaValor.FirstOrDefault(v => v.IdRubro == idRubro);
        if (existente == null)
        {
            if (!rubro.Activo)
            {
                throw new ReglaNegocioException($"El rubro '{rubro.Nombre}' está inactivo.", 409);
            }

            existente = new NominaValor { IdNominaEmpleado = fila.IdNominaEmpleado, IdRubro = idRubro };
            fila.NominaValor.Add(existente);
            _nominaRepositorio.AgregarValor(existente);
        }

        existente.Monto = monto;
        existente.Descripcion = Texto(descripcion);
    }

    private static void AsignarDias(NominaEmpleado fila, int dias)
    {
        if (dias < 0 || dias > fila.DiasPeriodo)
        {
            throw new ReglaNegocioException($"{fila.IdEmpleadoNavigation.CodigoEmpleado}: los días a pagar deben estar entre 0 y {fila.DiasPeriodo} (días del periodo).");
        }
        fila.DiasPagar = dias;
    }

    private static void AsignarHoras(NominaEmpleado fila, decimal horas)
    {
        if (horas < 0 || horas > 999.99m || horas != Math.Round(horas, 2))
        {
            throw new ReglaNegocioException($"{fila.IdEmpleadoNavigation.CodigoEmpleado}: las horas extra deben estar entre 0 y 999.99, con máximo 2 decimales.");
        }
        fila.HorasExtra = horas;
    }

    private static void AsignarIsr(NominaEmpleado fila, decimal isr)
    {
        if (isr < 0 || isr != Math.Round(isr, 2))
        {
            throw new ReglaNegocioException($"{fila.IdEmpleadoNavigation.CodigoEmpleado}: el ISR debe ser positivo y con máximo 2 decimales.");
        }
        fila.Isr = isr;
    }

    private static bool EstaAjustada(NominaEmpleado fila)
    {
        return fila.DiasPagar != fila.DiasPagarSugeridos;
    }

    private void EliminarFila(NominaEmpleado fila)
    {
        foreach (var rubro in fila.NominaRubro.ToList())
        {
            _nominaRepositorio.EliminarRubro(rubro);
        }
        foreach (var valor in fila.NominaValor.ToList())
        {
            _nominaRepositorio.EliminarValor(valor);
        }
        _nominaRepositorio.EliminarEmpleado(fila);
    }

    private static void LimpiarTotales(NominaEmpleado fila)
    {
        fila.TotalIngresos = 0;
        fila.TotalDescuentos = 0;
        fila.Liquido = 0;
        fila.TotalPatronal = 0;
    }

    private static void LimpiarTotales(Nomina nomina)
    {
        nomina.TotalIngresos = 0;
        nomina.TotalDescuentos = 0;
        nomina.TotalLiquido = 0;
        nomina.TotalPatronal = 0;
    }

    private static void ValidarFechaPago(DateOnly fechaPago, DateOnly inicio)
    {
        if (fechaPago < inicio || fechaPago > inicio.AddMonths(2))
        {
            throw new ReglaNegocioException("La fecha de pago debe estar entre el inicio del periodo y dos meses después.");
        }
    }

    private async Task<HashSet<int>> EmpleadosConPeriodo(Nomina nomina)
    {
        return (await _nominaRepositorio.ObtenerPeriodosEntre(nomina.FechaInicio, nomina.FechaFin))
            .Select(p => p.IdEmpleado)
            .ToHashSet();
    }

    private async Task<List<AdvertenciaDto>> CalcularAdvertencias(Nomina nomina, List<NominaEmpleado> empleados)
    {
        var advertencias = new List<AdvertenciaDto>();
        if (nomina.Estado == "CERRADA")
        {
            return advertencias;
        }

        var conPeriodo = await EmpleadosConPeriodo(nomina);
        foreach (var fila in empleados)
        {
            advertencias.AddRange(AdvertenciasFila(nomina, fila, conPeriodo));
        }

        var hoy = Hoy();
        if (nomina.FechaFin > hoy)
        {
            advertencias.Add(Advertencia("MES_NO_TERMINADO", "El mes todavía no termina; puede haber asistencias pendientes.", false, null));
        }

        var otras = await _nominaRepositorio.ObtenerCodigosDelMes(nomina.IdTipoPlanilla, nomina.Anio, nomina.Mes, nomina.IdNomina);
        if (otras.Count > 0)
        {
            advertencias.Add(Advertencia("OTRA_PLANILLA_MES",
                $"{(otras.Count == 1 ? "Hay otra planilla" : "Hay otras planillas")} de {nomina.Mes:00}/{nomina.Anio}: {string.Join(", ", otras)}.", false, null));
        }

        var masAntigua = await _nominaRepositorio.ObtenerInicioMasAntiguo(nomina.IdTipoPlanilla);
        var anterior = nomina.FechaInicio.AddMonths(-1);
        if (masAntigua < nomina.FechaInicio && !await _nominaRepositorio.ExisteTipoMes(nomina.IdTipoPlanilla, anterior.Year, anterior.Month))
        {
            advertencias.Add(Advertencia("MES_ANTERIOR_FALTANTE", $"Falta crear la planilla de {anterior:MM'/'yyyy}.", true, null));
        }

        if (nomina.Estado != "CREADA")
        {
            var faltantes = conPeriodo.Count(id => empleados.All(e => e.IdEmpleado != id));
            if (faltantes > 0)
            {
                advertencias.Add(Advertencia("EMPLEADOS_NUEVOS",
                    faltantes == 1
                        ? "Hay 1 empleado con periodo laboral que no está en la planilla; prepárela de nuevo para incluirlo."
                        : $"Hay {faltantes} empleados con periodo laboral que no están en la planilla; prepárela de nuevo para incluirlos.",
                    true, null));
            }
        }

        return advertencias;
    }

    private static List<AdvertenciaDto> AdvertenciasFila(Nomina nomina, NominaEmpleado fila, HashSet<int> conPeriodo)
    {
        var advertencias = new List<AdvertenciaDto>();
        if (nomina.Estado == "CERRADA")
        {
            return advertencias;
        }

        var codigo = fila.IdEmpleadoNavigation.CodigoEmpleado;
        if (!conPeriodo.Contains(fila.IdEmpleado))
        {
            advertencias.Add(Advertencia("SIN_PERIODO",
                $"{codigo}: ya no tiene periodo laboral en el mes; prepare la planilla de nuevo.", true, fila.IdEmpleado));
        }
        if (fila.Liquido < 0)
        {
            advertencias.Add(Advertencia("LIQUIDO_NEGATIVO",
                $"{codigo}: el líquido a recibir es negativo (Q{Monto(fila.Liquido)}).", true, fila.IdEmpleado));
        }
        if (nomina.DiasMes.HasValue && fila.DiasPeriodo < nomina.DiasMes.Value && fila.Isr > 0)
        {
            advertencias.Add(Advertencia("ISR_PERIODO_PARCIAL",
                $"{codigo}: trabajó parte del mes; se le retiene el ISR mensual completo (Q{Monto(fila.Isr)}).", false, fila.IdEmpleado));
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
        nomina.BonificacionIncentivo = parametros.BonificacionIncentivo;
        nomina.FactorHoraExtra = parametros.FactorHoraExtra;
        nomina.DiasMes = parametros.DiasMes;
        nomina.HorasMes = parametros.HorasMes;
    }

    private static string NombreMes(int mes)
    {
        return CultureInfo.GetCultureInfo("es-ES").DateTimeFormat.GetMonthName(mes);
    }

    private static string Texto(string valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }

    private static string Monto(decimal valor)
    {
        return valor.ToString("#,##0.00", CultureInfo.InvariantCulture);
    }

    private static DateOnly Hoy()
    {
        return DateOnly.FromDateTime(DateTime.Today);
    }

    private static NominaDto ADto(Nomina nomina, int cantidadEmpleados, List<AdvertenciaDto> advertencias)
    {
        return new NominaDto
        {
            IdNomina = nomina.IdNomina,
            IdTipoPlanilla = nomina.IdTipoPlanilla,
            CodigoTipoPlanilla = nomina.IdTipoPlanillaNavigation?.Codigo,
            NombreTipoPlanilla = nomina.IdTipoPlanillaNavigation?.Nombre,
            Codigo = nomina.Codigo,
            Descripcion = nomina.Descripcion,
            Anio = nomina.Anio,
            Mes = nomina.Mes,
            FechaInicio = nomina.FechaInicio,
            FechaFin = nomina.FechaFin,
            FechaPago = nomina.FechaPago,
            Comentario = nomina.Comentario,
            Estado = nomina.Estado,
            TotalIngresos = nomina.TotalIngresos,
            TotalDescuentos = nomina.TotalDescuentos,
            TotalLiquido = nomina.TotalLiquido,
            TotalPatronal = nomina.TotalPatronal,
            CantidadEmpleados = cantidadEmpleados,
            FechaPreparacion = nomina.FechaPreparacion,
            FechaGeneracion = nomina.FechaGeneracion,
            IgssLaboral = nomina.IgssLaboral,
            IgssPatronal = nomina.IgssPatronal,
            BonificacionIncentivo = nomina.BonificacionIncentivo,
            FactorHoraExtra = nomina.FactorHoraExtra,
            DiasMes = nomina.DiasMes,
            HorasMes = nomina.HorasMes,
            UsuarioCierre = nomina.UsuarioCierre,
            FechaCierre = nomina.FechaCierre,
            MotivoReapertura = nomina.MotivoReapertura,
            Version = nomina.Version == null ? null : Convert.ToBase64String(nomina.Version),
            PuedeCerrar = advertencias == null ? null : nomina.Estado == "GENERADA" && !advertencias.Any(a => a.Bloquea),
            Advertencias = advertencias
        };
    }

    private static NominaEmpleadoDto AEmpleadoDto(NominaEmpleado fila, List<AdvertenciaDto> advertencias)
    {
        return new NominaEmpleadoDto
        {
            IdNominaEmpleado = fila.IdNominaEmpleado,
            IdEmpleado = fila.IdEmpleado,
            CodigoEmpleado = fila.IdEmpleadoNavigation.CodigoEmpleado,
            NombreCompleto = $"{fila.IdEmpleadoNavigation.Nombres} {fila.IdEmpleadoNavigation.Apellidos}",
            IdPuesto = fila.IdPuesto,
            NombrePuesto = fila.IdPuestoNavigation?.Nombre,
            IdDepartamento = fila.IdDepartamento,
            NombreDepartamento = fila.IdDepartamentoNavigation?.Nombre,
            Salario = fila.Salario,
            RetencionIsr = fila.RetencionIsr,
            TipoPago = fila.TipoPago,
            Banco = fila.Banco,
            CuentaBancaria = fila.CuentaBancaria,
            DiasPeriodo = fila.DiasPeriodo,
            DiasFalta = fila.DiasFalta,
            DiasSuspension = fila.DiasSuspension,
            DiasPagarSugeridos = fila.DiasPagarSugeridos,
            DiasPagar = fila.DiasPagar,
            HorasExtraSugeridas = fila.HorasExtraSugeridas,
            HorasExtra = fila.HorasExtra,
            Isr = fila.Isr,
            DatosAjustados = fila.DatosAjustados,
            TotalIngresos = fila.TotalIngresos,
            TotalDescuentos = fila.TotalDescuentos,
            Liquido = fila.Liquido,
            TotalPatronal = fila.TotalPatronal,
            Valores = fila.NominaValor
                .OrderBy(v => v.IdRubroNavigation.Orden)
                .Select(v => new NominaValorDto
                {
                    IdRubro = v.IdRubro,
                    CodigoRubro = v.IdRubroNavigation.Codigo,
                    NombreRubro = v.IdRubroNavigation.Nombre,
                    Tipo = v.IdRubroNavigation.Tipo,
                    Monto = v.Monto,
                    Descripcion = v.Descripcion
                }).ToList(),
            Rubros = fila.NominaRubro.OrderBy(r => r.Orden).Select(ARubroDto).ToList(),
            Advertencias = advertencias
        };
    }

    private static NominaRubroDto ARubroDto(NominaRubro rubro)
    {
        return new NominaRubroDto
        {
            IdRubro = rubro.IdRubro,
            Codigo = rubro.IdRubroNavigation.Codigo,
            Nombre = rubro.IdRubroNavigation.Nombre,
            Tipo = rubro.IdRubroNavigation.Tipo,
            Orden = rubro.Orden,
            Base = rubro.Base,
            Monto = rubro.Monto,
            Descripcion = rubro.Descripcion
        };
    }
}
