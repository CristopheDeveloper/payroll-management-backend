using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Excepciones;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Servicios;

public class PuestoServicio : IPuestoServicio
{
    private readonly IPuestoRepositorio _puestoRepositorio;
    private readonly IEmpleadoRepositorio _empleadoRepositorio;
    private readonly IUnidadTrabajo _unidadTrabajo;

    public PuestoServicio(IPuestoRepositorio puestoRepositorio, IEmpleadoRepositorio empleadoRepositorio,
        IUnidadTrabajo unidadTrabajo)
    {
        _puestoRepositorio = puestoRepositorio;
        _empleadoRepositorio = empleadoRepositorio;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<List<PuestoDto>> ObtenerTodos(bool soloActivos)
    {
        var puestos = await _puestoRepositorio.ObtenerTodos(soloActivos);
        return puestos.Select(ADto).ToList();
    }

    public async Task<PuestoDto> ObtenerPorId(int idPuesto)
    {
        var puesto = await _puestoRepositorio.ObtenerPorId(idPuesto)
            ?? throw new ReglaNegocioException("El puesto no existe.", 404);

        return ADto(puesto);
    }

    public async Task<PuestoDto> Crear(PuestoGuardarDto dto)
    {
        var nombre = dto.Nombre.Trim();
        await ValidarNombreUnico(nombre, 0);

        var puesto = new Puesto
        {
            Nombre = nombre,
            Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
            Activo = true
        };

        _puestoRepositorio.Agregar(puesto);
        await _unidadTrabajo.GuardarCambiosAsync();

        return ADto(puesto);
    }

    public async Task<PuestoDto> Editar(int idPuesto, PuestoGuardarDto dto)
    {
        var puesto = await ObtenerParaModificar(idPuesto);
        var nombre = dto.Nombre.Trim();
        await ValidarNombreUnico(nombre, idPuesto);

        puesto.Nombre = nombre;
        puesto.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
        await _unidadTrabajo.GuardarCambiosAsync();

        return ADto(puesto);
    }

    public async Task Activar(int idPuesto)
    {
        var puesto = await ObtenerParaModificar(idPuesto);

        puesto.Activo = true;
        await _unidadTrabajo.GuardarCambiosAsync();
    }

    public async Task Desactivar(int idPuesto)
    {
        var puesto = await ObtenerParaModificar(idPuesto);
        if (await _empleadoRepositorio.ExistenActivosEnPuesto(idPuesto))
        {
            throw new ReglaNegocioException("No se puede desactivar el puesto porque tiene empleados activos asignados.", 409);
        }

        puesto.Activo = false;
        await _unidadTrabajo.GuardarCambiosAsync();
    }

    private async Task<Puesto> ObtenerParaModificar(int idPuesto)
    {
        return await _puestoRepositorio.ObtenerParaModificar(idPuesto)
            ?? throw new ReglaNegocioException("El puesto no existe.", 404);
    }

    private async Task ValidarNombreUnico(string nombre, int idExcluir)
    {
        if (await _puestoRepositorio.ExisteNombre(nombre, idExcluir))
        {
            throw new ReglaNegocioException($"Ya existe un puesto con el nombre '{nombre}'.", 409);
        }
    }

    private static PuestoDto ADto(Puesto puesto)
    {
        return new PuestoDto
        {
            IdPuesto = puesto.IdPuesto,
            Nombre = puesto.Nombre,
            Descripcion = puesto.Descripcion,
            Activo = puesto.Activo
        };
    }
}
