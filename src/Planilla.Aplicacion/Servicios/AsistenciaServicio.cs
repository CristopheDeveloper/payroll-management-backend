using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Excepciones;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;
using Planilla.Dominio.Tiempo;

namespace Planilla.Aplicacion.Servicios;

public class AsistenciaServicio : IAsistenciaServicio
{
    private readonly IAsistenciaRepositorio _asistenciaRepositorio;
    private readonly IEmpleadoRepositorio _empleadoRepositorio;
    private readonly INominaRepositorio _nominaRepositorio;
    private readonly IUnidadTrabajo _unidadTrabajo;

    public AsistenciaServicio(IAsistenciaRepositorio asistenciaRepositorio, IEmpleadoRepositorio empleadoRepositorio,
        INominaRepositorio nominaRepositorio, IUnidadTrabajo unidadTrabajo)
    {
        _asistenciaRepositorio = asistenciaRepositorio;
        _empleadoRepositorio = empleadoRepositorio;
        _nominaRepositorio = nominaRepositorio;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<DiaAsistenciaDto> ObtenerDia(DateOnly? fecha)
    {
        if (fecha == null)
        {
            throw new ReglaNegocioException("La fecha es obligatoria.");
        }
        ValidarFecha(fecha.Value);

        var empleados = await _asistenciaRepositorio.ObtenerEmpleadosConPeriodoEntre(fecha.Value, fecha.Value);
        var lista = await _asistenciaRepositorio.ObtenerPorFecha(fecha.Value);
        var asistencias = lista.ToDictionary(a => a.IdEmpleado);

        var filas = empleados.Select(e =>
        {
            asistencias.TryGetValue(e.IdEmpleado, out var asistencia);
            return new AsistenciaDiaDto
            {
                IdEmpleado = e.IdEmpleado,
                CodigoEmpleado = e.CodigoEmpleado,
                NombreCompleto = $"{e.Nombres} {e.Apellidos}",
                Departamento = e.IdDepartamentoNavigation.Nombre,
                IdAsistencia = asistencia?.IdAsistencia,
                Estado = asistencia?.Estado ?? "PRESENTE",
                HorasExtra = asistencia?.HorasExtra ?? 0,
                Observacion = asistencia?.Observacion
            };
        }).ToList();

        return new DiaAsistenciaDto
        {
            Fecha = fecha.Value,
            Version = CalcularVersion(lista),
            Empleados = filas
        };
    }

    public async Task<DiaAsistenciaDto> GuardarDia(LoteAsistenciaDto dto)
    {
        var fecha = dto.Fecha.Value;
        ValidarFecha(fecha);
        await PlanillaCerrada.Validar(_nominaRepositorio, fecha, fecha);

        if (dto.Registros.Any(r => r == null))
        {
            throw new ReglaNegocioException("Hay registros vacíos en la lista.");
        }

        if (dto.Registros.Select(r => r.IdEmpleado).Distinct().Count() != dto.Registros.Count)
        {
            throw new ReglaNegocioException("Hay empleados repetidos en la lista.");
        }

        var empleados = (await _asistenciaRepositorio.ObtenerEmpleadosConPeriodoEntre(fecha, fecha))
            .ToDictionary(e => e.IdEmpleado);
        var fuera = dto.Registros.Where(r => !empleados.ContainsKey(r.IdEmpleado)).Select(r => r.IdEmpleado).ToList();
        if (fuera.Count > 0)
        {
            throw new ReglaNegocioException($"Estos empleados no estaban trabajando el {fecha:dd'/'MM'/'yyyy} (Id): {string.Join(", ", fuera)}.");
        }

        foreach (var registro in dto.Registros)
        {
            var codigo = empleados[registro.IdEmpleado].CodigoEmpleado;
            if (registro.HorasExtra != Math.Round(registro.HorasExtra, 2))
            {
                throw new ReglaNegocioException($"{codigo}: las horas extra deben tener como máximo 2 decimales.");
            }
            if (registro.HorasExtra > 0 && registro.Estado != "PRESENTE")
            {
                throw new ReglaNegocioException($"{codigo}: solo se pueden registrar horas extra con estado PRESENTE.");
            }
        }

        var guardadas = await _asistenciaRepositorio.ObtenerPorFechaParaModificar(fecha);
        if (CalcularVersion(guardadas) != dto.Version)
        {
            throw new ReglaNegocioException("Otra persona modificó las asistencias de este día. Recargue para ver los cambios antes de guardar.", 409);
        }
        var existentes = guardadas.ToDictionary(a => a.IdEmpleado);

        foreach (var registro in dto.Registros)
        {
            var observacion = string.IsNullOrWhiteSpace(registro.Observacion) ? null : registro.Observacion.Trim();

            if (existentes.TryGetValue(registro.IdEmpleado, out var asistencia))
            {
                asistencia.Estado = registro.Estado;
                asistencia.HorasExtra = registro.HorasExtra;
                asistencia.Observacion = observacion;
            }
            else
            {
                _asistenciaRepositorio.Agregar(new Asistencia
                {
                    IdEmpleado = registro.IdEmpleado,
                    Fecha = fecha,
                    Estado = registro.Estado,
                    HorasExtra = registro.HorasExtra,
                    Observacion = observacion
                });
            }
        }

        await _unidadTrabajo.GuardarCambiosAsync();

        return await ObtenerDia(fecha);
    }

    public async Task<List<ResumenAsistenciaDto>> ObtenerResumen(int? anio, int? mes)
    {
        var (desde, hasta) = RangoDelMes(anio, mes);

        var empleados = await _asistenciaRepositorio.ObtenerEmpleadosConPeriodoEntre(desde, hasta);
        var totales = (await _asistenciaRepositorio.ObtenerTotales(desde, hasta))
            .ToDictionary(t => t.IdEmpleado);

        return empleados.Select(e =>
        {
            totales.TryGetValue(e.IdEmpleado, out var total);
            return new ResumenAsistenciaDto
            {
                IdEmpleado = e.IdEmpleado,
                CodigoEmpleado = e.CodigoEmpleado,
                NombreCompleto = $"{e.Nombres} {e.Apellidos}",
                Faltas = total?.Faltas ?? 0,
                Permisos = total?.Permisos ?? 0,
                Vacaciones = total?.Vacaciones ?? 0,
                SuspensionIgss = total?.SuspensionIgss ?? 0,
                HorasExtra = total?.HorasExtra ?? 0
            };
        }).ToList();
    }

    public async Task<List<AsistenciaDto>> ObtenerPorEmpleado(int idEmpleado, int? anio, int? mes)
    {
        var (desde, hasta) = RangoDelMes(anio, mes);

        if (await _empleadoRepositorio.ObtenerPorId(idEmpleado) == null)
        {
            throw new ReglaNegocioException("El empleado no existe.", 404);
        }

        var asistencias = await _asistenciaRepositorio.ObtenerPorEmpleado(idEmpleado, desde, hasta);
        return asistencias.Select(ADto).ToList();
    }

    public async Task<List<AsistenciaDto>> RegistrarRango(RangoAsistenciaDto dto)
    {
        var desde = dto.Desde.Value;
        var hasta = dto.Hasta.Value;
        if (hasta < desde)
        {
            throw new ReglaNegocioException("La fecha final no puede ser anterior a la fecha inicial.");
        }
        if (hasta.DayNumber - desde.DayNumber > 365)
        {
            throw new ReglaNegocioException("El rango no puede ser mayor a un año.");
        }
        if (hasta > Hoy().AddYears(1))
        {
            throw new ReglaNegocioException("No se pueden registrar ausencias con más de un año de anticipación.");
        }

        var empleado = await _empleadoRepositorio.ObtenerPorId(dto.IdEmpleado)
            ?? throw new ReglaNegocioException("El empleado no existe.", 404);

        var omitirFinesDeSemana = dto.SoloDiasHabiles && dto.Estado != "SUSPENSION_IGSS";
        var dias = new List<DateOnly>();
        for (var dia = desde; dia <= hasta; dia = dia.AddDays(1))
        {
            if (omitirFinesDeSemana && dia.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                continue;
            }
            dias.Add(dia);
        }
        if (dias.Count == 0)
        {
            throw new ReglaNegocioException("El rango no tiene días hábiles.");
        }
        await PlanillaCerrada.Validar(_nominaRepositorio, dias[0], dias[^1]);

        var fuera = dias
            .Where(d => !empleado.PeriodoLaboral.Any(p => p.FechaIngreso <= d && (p.FechaBaja == null || p.FechaBaja >= d)))
            .ToList();
        if (fuera.Count > 0)
        {
            throw new ReglaNegocioException($"El empleado no tiene periodo laboral en {fuera.Count} días del rango (desde el {fuera[0]:dd'/'MM'/'yyyy}).");
        }

        var observacion = string.IsNullOrWhiteSpace(dto.Observacion) ? null : dto.Observacion.Trim();
        var existentes = (await _asistenciaRepositorio.ObtenerPorEmpleadoParaModificar(dto.IdEmpleado, desde, hasta))
            .ToDictionary(a => a.Fecha);

        foreach (var dia in dias)
        {
            if (existentes.TryGetValue(dia, out var asistencia))
            {
                asistencia.Estado = dto.Estado;
                asistencia.HorasExtra = 0;
                asistencia.Observacion = observacion;
            }
            else
            {
                _asistenciaRepositorio.Agregar(new Asistencia
                {
                    IdEmpleado = dto.IdEmpleado,
                    Fecha = dia,
                    Estado = dto.Estado,
                    HorasExtra = 0,
                    Observacion = observacion
                });
            }
        }

        await _unidadTrabajo.GuardarCambiosAsync();

        var guardadas = await _asistenciaRepositorio.ObtenerPorEmpleado(dto.IdEmpleado, desde, hasta);
        return guardadas.Select(ADto).ToList();
    }

    public async Task<VacacionesAnioDto> ObtenerVacaciones(int idEmpleado, int? anio)
    {
        ValidarAnio(anio);

        if (await _empleadoRepositorio.ObtenerPorId(idEmpleado) == null)
        {
            throw new ReglaNegocioException("El empleado no existe.", 404);
        }

        var fechas = await _asistenciaRepositorio.ObtenerFechasPorEstado(idEmpleado, "VACACIONES",
            new DateOnly(anio.Value, 1, 1), new DateOnly(anio.Value, 12, 31));
        var hoy = Hoy();

        return new VacacionesAnioDto
        {
            Anio = anio.Value,
            DiasTomados = fechas.Count(f => f <= hoy),
            DiasProgramados = fechas.Count(f => f > hoy)
        };
    }

    public async Task Eliminar(int idAsistencia)
    {
        var asistencia = await _asistenciaRepositorio.ObtenerParaModificar(idAsistencia)
            ?? throw new ReglaNegocioException("La asistencia no existe.", 404);
        await PlanillaCerrada.Validar(_nominaRepositorio, asistencia.Fecha, asistencia.Fecha);

        _asistenciaRepositorio.Eliminar(asistencia);
        await _unidadTrabajo.GuardarCambiosAsync();
    }

    private static DateOnly Hoy()
    {
        return RelojGuatemala.Hoy;
    }

    private static AsistenciaDto ADto(Asistencia asistencia)
    {
        return new AsistenciaDto
        {
            IdAsistencia = asistencia.IdAsistencia,
            Fecha = asistencia.Fecha,
            Estado = asistencia.Estado,
            HorasExtra = asistencia.HorasExtra,
            Observacion = asistencia.Observacion
        };
    }

    private static void ValidarFecha(DateOnly fecha)
    {
        if (fecha > Hoy())
        {
            throw new ReglaNegocioException("No se pueden consultar ni registrar asistencias de fechas futuras.");
        }
    }

    private static string CalcularVersion(List<Asistencia> asistencias)
    {
        if (asistencias.Count == 0)
        {
            return "0";
        }

        var ultimoCambio = asistencias.Max(a => a.FechaAct ?? a.FechaIng);
        return $"{asistencias.Count}-{ultimoCambio.Ticks}";
    }

    private static void ValidarAnio(int? anio)
    {
        if (anio is null or < 2000 or > 2100)
        {
            throw new ReglaNegocioException("El año debe estar entre 2000 y 2100.");
        }
    }

    private static (DateOnly Desde, DateOnly Hasta) RangoDelMes(int? anio, int? mes)
    {
        ValidarAnio(anio);
        if (mes is null or < 1 or > 12)
        {
            throw new ReglaNegocioException("El mes debe estar entre 1 y 12.");
        }

        var desde = new DateOnly(anio.Value, mes.Value, 1);
        return (desde, desde.AddMonths(1).AddDays(-1));
    }
}
