using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Excepciones;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Servicios;

public class MovimientoNominaServicio : IMovimientoNominaServicio
{
    private readonly INominaRepositorio _nominaRepositorio;
    private readonly IRubroRepositorio _rubroRepositorio;
    private readonly IUnidadTrabajo _unidadTrabajo;

    public MovimientoNominaServicio(INominaRepositorio nominaRepositorio, IRubroRepositorio rubroRepositorio,
        IUnidadTrabajo unidadTrabajo)
    {
        _nominaRepositorio = nominaRepositorio;
        _rubroRepositorio = rubroRepositorio;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<List<MovimientoDto>> ObtenerTodos(int idNomina, int? idEmpleado)
    {
        if (await _nominaRepositorio.ObtenerPorId(idNomina) == null)
        {
            throw new ReglaNegocioException("La planilla no existe.", 404);
        }

        var movimientos = await _nominaRepositorio.ObtenerMovimientos(idNomina, idEmpleado);
        return movimientos.Select(ADto).ToList();
    }

    public async Task<MovimientoDto> Crear(int idNomina, MovimientoGuardarDto dto)
    {
        var nomina = await ObtenerAbierta(idNomina);
        await ValidarEmpleado(nomina, dto.IdEmpleado);
        await ValidarRubro(dto.IdRubro, null);
        ValidarMonto(dto.Monto);

        var movimiento = new MovimientoNomina
        {
            IdNomina = idNomina,
            IdEmpleado = dto.IdEmpleado,
            IdRubro = dto.IdRubro,
            Monto = dto.Monto,
            Descripcion = Limpiar(dto.Descripcion)
        };

        _nominaRepositorio.AgregarMovimiento(movimiento);
        _nominaRepositorio.MarcarModificada(nomina);
        await _unidadTrabajo.GuardarCambiosAsync();

        return ADto(await _nominaRepositorio.ObtenerMovimiento(movimiento.IdMovimiento));
    }

    public async Task<MovimientoDto> Editar(int idNomina, int idMovimiento, MovimientoGuardarDto dto)
    {
        var nomina = await ObtenerAbierta(idNomina);
        var movimiento = await ObtenerMovimientoParaModificar(idNomina, idMovimiento);

        if (dto.IdEmpleado != movimiento.IdEmpleado)
        {
            await ValidarEmpleado(nomina, dto.IdEmpleado);
        }
        await ValidarRubro(dto.IdRubro, movimiento.IdRubro);
        ValidarMonto(dto.Monto);

        movimiento.IdEmpleado = dto.IdEmpleado;
        movimiento.IdRubro = dto.IdRubro;
        movimiento.Monto = dto.Monto;
        movimiento.Descripcion = Limpiar(dto.Descripcion);
        _nominaRepositorio.MarcarModificada(nomina);
        await _unidadTrabajo.GuardarCambiosAsync();

        return ADto(await _nominaRepositorio.ObtenerMovimiento(idMovimiento));
    }

    public async Task Eliminar(int idNomina, int idMovimiento)
    {
        var nomina = await ObtenerAbierta(idNomina);
        var movimiento = await ObtenerMovimientoParaModificar(idNomina, idMovimiento);

        _nominaRepositorio.EliminarMovimiento(movimiento);
        _nominaRepositorio.MarcarModificada(nomina);
        await _unidadTrabajo.GuardarCambiosAsync();
    }

    private async Task<Nomina> ObtenerAbierta(int idNomina)
    {
        var nomina = await _nominaRepositorio.ObtenerParaModificar(idNomina)
            ?? throw new ReglaNegocioException("La planilla no existe.", 404);

        if (nomina.Estado == "CERRADA")
        {
            throw new ReglaNegocioException("La planilla está cerrada; no se pueden modificar sus movimientos.", 409);
        }

        return nomina;
    }

    private async Task<MovimientoNomina> ObtenerMovimientoParaModificar(int idNomina, int idMovimiento)
    {
        var movimiento = await _nominaRepositorio.ObtenerMovimientoParaModificar(idMovimiento);
        if (movimiento == null || movimiento.IdNomina != idNomina)
        {
            throw new ReglaNegocioException("El movimiento no existe en esta planilla.", 404);
        }

        return movimiento;
    }

    private async Task ValidarEmpleado(Nomina nomina, int idEmpleado)
    {
        if (!await _nominaRepositorio.TienePeriodoEntre(idEmpleado, nomina.FechaInicio, nomina.FechaFin))
        {
            throw new ReglaNegocioException("El empleado no tiene periodo laboral en el mes de la planilla.");
        }
    }

    private async Task ValidarRubro(int idRubro, int? idRubroActual)
    {
        if (idRubro == idRubroActual)
        {
            return;
        }

        var rubro = await _rubroRepositorio.ObtenerPorId(idRubro);
        if (rubro == null || !rubro.Activo)
        {
            throw new ReglaNegocioException("El rubro no existe o está inactivo.");
        }
    }

    private static void ValidarMonto(decimal monto)
    {
        if (monto != Math.Round(monto, 2))
        {
            throw new ReglaNegocioException("El monto debe tener como máximo 2 decimales.");
        }
    }

    private static string Limpiar(string texto)
    {
        return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    }

    private static MovimientoDto ADto(MovimientoNomina movimiento)
    {
        return new MovimientoDto
        {
            IdMovimiento = movimiento.IdMovimiento,
            IdEmpleado = movimiento.IdEmpleado,
            CodigoEmpleado = movimiento.IdEmpleadoNavigation.CodigoEmpleado,
            NombreCompleto = $"{movimiento.IdEmpleadoNavigation.Nombres} {movimiento.IdEmpleadoNavigation.Apellidos}",
            IdRubro = movimiento.IdRubro,
            Rubro = movimiento.IdRubroNavigation.Nombre,
            Tipo = movimiento.IdRubroNavigation.Tipo,
            AfectoIgss = movimiento.IdRubroNavigation.AfectoIgss,
            Descripcion = movimiento.Descripcion,
            Monto = movimiento.Monto
        };
    }
}
