using System.ComponentModel.DataAnnotations;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Excepciones;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;
using Planilla.Dominio.Tiempo;

namespace Planilla.Aplicacion.Servicios;

public class EmpleadoServicio : IEmpleadoServicio
{
    private readonly IEmpleadoRepositorio _empleadoRepositorio;
    private readonly IPuestoRepositorio _puestoRepositorio;
    private readonly IDepartamentoRepositorio _departamentoRepositorio;
    private readonly IAsistenciaRepositorio _asistenciaRepositorio;
    private readonly INominaRepositorio _nominaRepositorio;
    private readonly IUnidadTrabajo _unidadTrabajo;

    public EmpleadoServicio(IEmpleadoRepositorio empleadoRepositorio, IPuestoRepositorio puestoRepositorio,
        IDepartamentoRepositorio departamentoRepositorio, IAsistenciaRepositorio asistenciaRepositorio,
        INominaRepositorio nominaRepositorio, IUnidadTrabajo unidadTrabajo)
    {
        _empleadoRepositorio = empleadoRepositorio;
        _puestoRepositorio = puestoRepositorio;
        _departamentoRepositorio = departamentoRepositorio;
        _asistenciaRepositorio = asistenciaRepositorio;
        _nominaRepositorio = nominaRepositorio;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<List<EmpleadoDto>> ObtenerTodos(bool? activo, int? idDepartamento, int? idPuesto, string buscar)
    {
        var empleados = await _empleadoRepositorio.ObtenerTodos(activo, idDepartamento, idPuesto, buscar);
        return empleados.Select(ADto).ToList();
    }

    public async Task<EmpleadoDetalleDto> ObtenerPorId(int idEmpleado)
    {
        var empleado = await _empleadoRepositorio.ObtenerPorId(idEmpleado)
            ?? throw new ReglaNegocioException("El empleado no existe.", 404);

        return ADetalleDto(empleado);
    }

    public async Task<EmpleadoDetalleDto> Contratar(EmpleadoCrearDto dto)
    {
        var dpi = dto.Dpi.Trim();
        var existente = await _empleadoRepositorio.ObtenerPorDpi(dpi, 0);
        if (existente != null)
        {
            throw new ReglaNegocioException(existente.Activo
                ? $"Ya existe un empleado activo con ese DPI ({existente.CodigoEmpleado})."
                : $"El DPI pertenece a {existente.CodigoEmpleado}, que está de baja; use Reingresar.", 409);
        }

        await ValidarPuesto(dto.IdPuesto, null);
        await ValidarDepartamento(dto.IdDepartamento, null);
        ValidarDatos(dto);

        var fechaIngreso = dto.FechaIngreso.Value;
        if (fechaIngreso > Hoy())
        {
            throw new ReglaNegocioException("La fecha de ingreso no puede ser futura.");
        }
        if (fechaIngreso <= dto.FechaNacimiento.Value)
        {
            throw new ReglaNegocioException("La fecha de ingreso debe ser posterior a la fecha de nacimiento.");
        }
        await PlanillaCerrada.Validar(_nominaRepositorio, fechaIngreso, DateOnly.MaxValue);

        var empleado = new Empleado { Activo = true };
        AsignarDatos(empleado, dto, dpi);
        empleado.PeriodoLaboral.Add(new PeriodoLaboral { FechaIngreso = fechaIngreso });

        _empleadoRepositorio.Agregar(empleado);
        await _unidadTrabajo.GuardarCambiosAsync();

        return await ObtenerPorId(empleado.IdEmpleado);
    }

    public async Task<EmpleadoDetalleDto> Editar(int idEmpleado, EmpleadoEditarDto dto)
    {
        var empleado = await ObtenerParaModificar(idEmpleado);

        var dpi = dto.Dpi.Trim();
        var otro = await _empleadoRepositorio.ObtenerPorDpi(dpi, idEmpleado);
        if (otro != null)
        {
            throw new ReglaNegocioException($"El DPI ya pertenece a {otro.CodigoEmpleado}.", 409);
        }

        await ValidarPuesto(dto.IdPuesto, empleado.IdPuesto);
        await ValidarDepartamento(dto.IdDepartamento, empleado.IdDepartamento);
        ValidarDatos(dto);

        var periodos = await _empleadoRepositorio.ObtenerPeriodosParaModificar(idEmpleado);
        if (periodos.Count > 0)
        {
            var primerIngreso = periodos.Min(p => p.FechaIngreso);
            if (dto.FechaNacimiento.Value >= primerIngreso)
            {
                throw new ReglaNegocioException($"La fecha de nacimiento debe ser anterior a la primera fecha de ingreso ({primerIngreso:dd'/'MM'/'yyyy}).");
            }
        }

        AsignarDatos(empleado, dto, dpi);
        await _unidadTrabajo.GuardarCambiosAsync();

        return await ObtenerPorId(idEmpleado);
    }

    public async Task<EmpleadoDetalleDto> DarDeBaja(int idEmpleado, BajaDto dto)
    {
        var empleado = await ObtenerParaModificar(idEmpleado);
        var periodos = await _empleadoRepositorio.ObtenerPeriodosParaModificar(idEmpleado);
        var abierto = periodos.FirstOrDefault(p => p.FechaBaja == null)
            ?? throw new ReglaNegocioException("El empleado ya está de baja.", 409);

        var fechaBaja = dto.FechaBaja.Value;
        if (fechaBaja > Hoy())
        {
            throw new ReglaNegocioException("La fecha de baja no puede ser futura.");
        }
        if (fechaBaja < abierto.FechaIngreso)
        {
            throw new ReglaNegocioException($"La fecha de baja no puede ser anterior a la fecha de ingreso ({abierto.FechaIngreso:dd'/'MM'/'yyyy}).");
        }

        var motivo = Limpiar(dto.MotivoBaja);
        if (dto.TipoBaja == "OTRO" && motivo == null)
        {
            throw new ReglaNegocioException("El motivo es obligatorio cuando el tipo de baja es OTRO.");
        }
        await PlanillaCerrada.Validar(_nominaRepositorio, fechaBaja, DateOnly.MaxValue);

        var hoy = Hoy();
        var posteriores = await _asistenciaRepositorio.ObtenerFechas(idEmpleado, fechaBaja.AddDays(1), null);
        var pasadas = posteriores.Where(f => f <= hoy).ToList();
        if (pasadas.Count > 0)
        {
            throw new ReglaNegocioException($"El empleado tiene {pasadas.Count} asistencias registradas entre la fecha de baja y hoy (desde el {pasadas[0]:dd'/'MM'/'yyyy}). Revíselas y elimínelas antes de dar de baja.", 409);
        }

        var programadas = posteriores.Count - pasadas.Count;
        if (programadas > 0)
        {
            if (!dto.EliminarAusenciasProgramadas)
            {
                var primera = posteriores.First(f => f > hoy);
                throw new ReglaNegocioException($"El empleado tiene {programadas} ausencias programadas después de hoy (desde el {primera:dd'/'MM'/'yyyy}). Confirme para eliminarlas y dar de baja.", 409, "AUSENCIAS_PROGRAMADAS");
            }

            var futuras = await _asistenciaRepositorio.ObtenerPorEmpleadoParaModificar(idEmpleado, hoy.AddDays(1), DateOnly.MaxValue);
            foreach (var asistencia in futuras)
            {
                _asistenciaRepositorio.Eliminar(asistencia);
            }
        }

        abierto.FechaBaja = fechaBaja;
        abierto.TipoBaja = dto.TipoBaja;
        abierto.MotivoBaja = motivo;
        empleado.Activo = false;
        await _unidadTrabajo.GuardarCambiosAsync();

        return await ObtenerPorId(idEmpleado);
    }

    public async Task<EmpleadoDetalleDto> Reingresar(int idEmpleado, ReingresoDto dto)
    {
        var empleado = await ObtenerParaModificar(idEmpleado);
        var periodos = await _empleadoRepositorio.ObtenerPeriodosParaModificar(idEmpleado);
        if (periodos.Any(p => p.FechaBaja == null))
        {
            throw new ReglaNegocioException("El empleado está activo; no se puede reingresar.", 409);
        }

        var fechaIngreso = dto.FechaIngreso.Value;
        if (fechaIngreso > Hoy())
        {
            throw new ReglaNegocioException("La fecha de ingreso no puede ser futura.");
        }

        var ultimo = periodos.FirstOrDefault();
        if (ultimo != null && fechaIngreso <= ultimo.FechaBaja.Value)
        {
            throw new ReglaNegocioException($"La fecha de ingreso debe ser posterior a la última fecha de baja ({ultimo.FechaBaja.Value:dd'/'MM'/'yyyy}).");
        }
        await PlanillaCerrada.Validar(_nominaRepositorio, fechaIngreso, DateOnly.MaxValue);

        await ValidarPuesto(dto.IdPuesto, null);
        await ValidarDepartamento(dto.IdDepartamento, null);
        ValidarDosDecimales(dto.Salario, "El salario");
        if (empleado.RetencionIsr >= dto.Salario)
        {
            throw new ReglaNegocioException($"La retención de ISR del empleado ({empleado.RetencionIsr:0.00}) debe ser menor al salario; corríjala primero con Editar.");
        }

        empleado.IdPuesto = dto.IdPuesto;
        empleado.IdDepartamento = dto.IdDepartamento;
        empleado.Salario = dto.Salario;
        empleado.Activo = true;
        _empleadoRepositorio.AgregarPeriodo(new PeriodoLaboral
        {
            IdEmpleado = empleado.IdEmpleado,
            FechaIngreso = fechaIngreso
        });
        await _unidadTrabajo.GuardarCambiosAsync();

        return await ObtenerPorId(idEmpleado);
    }

    public async Task<EmpleadoDetalleDto> CorregirFechaIngreso(int idEmpleado, CorregirFechaIngresoDto dto)
    {
        var empleado = await ObtenerParaModificar(idEmpleado);
        var periodos = await _empleadoRepositorio.ObtenerPeriodosParaModificar(idEmpleado);
        var ultimo = periodos.FirstOrDefault()
            ?? throw new ReglaNegocioException("El empleado no tiene periodos laborales.", 409);
        var anterior = periodos.Skip(1).FirstOrDefault();

        var fechaIngreso = dto.FechaIngreso.Value;
        if (fechaIngreso > Hoy())
        {
            throw new ReglaNegocioException("La fecha de ingreso no puede ser futura.");
        }
        if (fechaIngreso <= empleado.FechaNacimiento)
        {
            throw new ReglaNegocioException("La fecha de ingreso debe ser posterior a la fecha de nacimiento.");
        }
        if (anterior != null && fechaIngreso <= anterior.FechaBaja.Value)
        {
            throw new ReglaNegocioException($"La fecha de ingreso debe ser posterior a la baja del periodo anterior ({anterior.FechaBaja.Value:dd'/'MM'/'yyyy}).");
        }
        if (ultimo.FechaBaja.HasValue && fechaIngreso > ultimo.FechaBaja.Value)
        {
            throw new ReglaNegocioException($"La fecha de ingreso no puede ser posterior a la fecha de baja del periodo ({ultimo.FechaBaja.Value:dd'/'MM'/'yyyy}).");
        }
        await PlanillaCerrada.Validar(_nominaRepositorio, fechaIngreso < ultimo.FechaIngreso ? fechaIngreso : ultimo.FechaIngreso, DateOnly.MaxValue);

        if (fechaIngreso > ultimo.FechaIngreso)
        {
            var anteriores = await _asistenciaRepositorio.ObtenerFechas(idEmpleado, ultimo.FechaIngreso, fechaIngreso.AddDays(-1));
            if (anteriores.Count > 0)
            {
                throw new ReglaNegocioException($"El empleado tiene {anteriores.Count} asistencias registradas antes de la nueva fecha de ingreso (desde el {anteriores[0]:dd'/'MM'/'yyyy}). Elimínelas antes de corregir la fecha.", 409);
            }
        }

        ultimo.FechaIngreso = fechaIngreso;
        await _unidadTrabajo.GuardarCambiosAsync();

        return await ObtenerPorId(idEmpleado);
    }

    private async Task<Empleado> ObtenerParaModificar(int idEmpleado)
    {
        return await _empleadoRepositorio.ObtenerParaModificar(idEmpleado)
            ?? throw new ReglaNegocioException("El empleado no existe.", 404);
    }

    private async Task ValidarPuesto(int idPuesto, int? idPuestoActual)
    {
        if (idPuesto == idPuestoActual)
        {
            return;
        }

        var puesto = await _puestoRepositorio.ObtenerPorId(idPuesto);
        if (puesto == null || !puesto.Activo)
        {
            throw new ReglaNegocioException("El puesto no existe o está inactivo.");
        }
    }

    private async Task ValidarDepartamento(int idDepartamento, int? idDepartamentoActual)
    {
        if (idDepartamento == idDepartamentoActual)
        {
            return;
        }

        var departamento = await _departamentoRepositorio.ObtenerPorId(idDepartamento);
        if (departamento == null || !departamento.Activo)
        {
            throw new ReglaNegocioException("El departamento no existe o está inactivo.");
        }
    }

    private static void ValidarDatos(EmpleadoEditarDto dto)
    {
        if (dto.FechaNacimiento.Value >= Hoy())
        {
            throw new ReglaNegocioException("La fecha de nacimiento debe ser anterior a hoy.");
        }

        if (dto.TipoPago == "TRANSFERENCIA"
            && (string.IsNullOrWhiteSpace(dto.Banco) || string.IsNullOrWhiteSpace(dto.CuentaBancaria)))
        {
            throw new ReglaNegocioException("Para pagar por transferencia el banco y la cuenta bancaria son obligatorios.");
        }

        ValidarDosDecimales(dto.Salario, "El salario");
        ValidarDosDecimales(dto.RetencionIsr, "La retención de ISR");
        if (dto.RetencionIsr >= dto.Salario)
        {
            throw new ReglaNegocioException("La retención de ISR debe ser menor al salario.");
        }

        var correo = Limpiar(dto.Correo);
        if (correo != null && !new EmailAddressAttribute().IsValid(correo))
        {
            throw new ReglaNegocioException("El correo no tiene un formato válido.");
        }
    }

    private static void ValidarDosDecimales(decimal monto, string campo)
    {
        if (monto != Math.Round(monto, 2))
        {
            throw new ReglaNegocioException($"{campo} debe tener como máximo 2 decimales.");
        }
    }

    private static void AsignarDatos(Empleado empleado, EmpleadoEditarDto dto, string dpi)
    {
        empleado.Nombres = dto.Nombres.Trim();
        empleado.Apellidos = dto.Apellidos.Trim();
        empleado.Dpi = dpi;
        empleado.Pasaporte = Limpiar(dto.Pasaporte);
        empleado.Nit = Limpiar(dto.Nit);
        empleado.NumeroIgss = Limpiar(dto.NumeroIgss);
        empleado.FechaNacimiento = dto.FechaNacimiento.Value;
        empleado.Genero = dto.Genero;
        empleado.Direccion = dto.Direccion.Trim();
        empleado.Telefono = Limpiar(dto.Telefono);
        empleado.Correo = Limpiar(dto.Correo);
        empleado.TipoPago = dto.TipoPago;
        empleado.Banco = Limpiar(dto.Banco);
        empleado.CuentaBancaria = Limpiar(dto.CuentaBancaria);
        empleado.IdPuesto = dto.IdPuesto;
        empleado.IdDepartamento = dto.IdDepartamento;
        empleado.Salario = dto.Salario;
        empleado.RetencionIsr = dto.RetencionIsr;
    }

    private static string Limpiar(string texto)
    {
        return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    }

    private static DateOnly Hoy()
    {
        return RelojGuatemala.Hoy;
    }

    private static PeriodoLaboral UltimoPeriodo(Empleado empleado)
    {
        return empleado.PeriodoLaboral
            .OrderByDescending(p => p.FechaIngreso)
            .ThenByDescending(p => p.IdPeriodoLaboral)
            .FirstOrDefault();
    }

    private static EmpleadoDto ADto(Empleado empleado)
    {
        var ultimo = UltimoPeriodo(empleado);
        return new EmpleadoDto
        {
            IdEmpleado = empleado.IdEmpleado,
            CodigoEmpleado = empleado.CodigoEmpleado,
            NombreCompleto = $"{empleado.Nombres} {empleado.Apellidos}",
            Dpi = empleado.Dpi,
            Puesto = empleado.IdPuestoNavigation.Nombre,
            Departamento = empleado.IdDepartamentoNavigation.Nombre,
            TipoPago = empleado.TipoPago,
            Salario = empleado.Salario,
            Activo = empleado.Activo,
            FechaIngresoActual = ultimo?.FechaIngreso,
            FechaBajaUltima = ultimo?.FechaBaja
        };
    }

    private static EmpleadoDetalleDto ADetalleDto(Empleado empleado)
    {
        return new EmpleadoDetalleDto
        {
            IdEmpleado = empleado.IdEmpleado,
            CodigoEmpleado = empleado.CodigoEmpleado,
            Nombres = empleado.Nombres,
            Apellidos = empleado.Apellidos,
            Dpi = empleado.Dpi,
            Pasaporte = empleado.Pasaporte,
            Nit = empleado.Nit,
            NumeroIgss = empleado.NumeroIgss,
            FechaNacimiento = empleado.FechaNacimiento,
            Genero = empleado.Genero,
            Direccion = empleado.Direccion,
            Telefono = empleado.Telefono,
            Correo = empleado.Correo,
            TipoPago = empleado.TipoPago,
            Banco = empleado.Banco,
            CuentaBancaria = empleado.CuentaBancaria,
            IdPuesto = empleado.IdPuesto,
            NombrePuesto = empleado.IdPuestoNavigation.Nombre,
            IdDepartamento = empleado.IdDepartamento,
            NombreDepartamento = empleado.IdDepartamentoNavigation.Nombre,
            Salario = empleado.Salario,
            RetencionIsr = empleado.RetencionIsr,
            Activo = empleado.Activo,
            Periodos = empleado.PeriodoLaboral
                .OrderByDescending(p => p.FechaIngreso)
                .ThenByDescending(p => p.IdPeriodoLaboral)
                .Select(p => new PeriodoLaboralDto
                {
                    IdPeriodoLaboral = p.IdPeriodoLaboral,
                    FechaIngreso = p.FechaIngreso,
                    FechaBaja = p.FechaBaja,
                    TipoBaja = p.TipoBaja,
                    MotivoBaja = p.MotivoBaja
                })
                .ToList()
        };
    }
}
