using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Excepciones;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Servicios;

public class DepartamentoServicio : IDepartamentoServicio
{
    private readonly IDepartamentoRepositorio _departamentoRepositorio;
    private readonly IUnidadTrabajo _unidadTrabajo;

    public DepartamentoServicio(IDepartamentoRepositorio departamentoRepositorio, IUnidadTrabajo unidadTrabajo)
    {
        _departamentoRepositorio = departamentoRepositorio;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<List<DepartamentoDto>> ObtenerTodos(bool soloActivos)
    {
        var departamentos = await _departamentoRepositorio.ObtenerTodos(soloActivos);
        return departamentos.Select(ADto).ToList();
    }

    public async Task<DepartamentoDto> ObtenerPorId(int idDepartamento)
    {
        var departamento = await _departamentoRepositorio.ObtenerPorId(idDepartamento)
            ?? throw new ReglaNegocioException("El departamento no existe.", 404);

        return ADto(departamento);
    }

    public async Task<DepartamentoDto> Crear(DepartamentoGuardarDto dto)
    {
        var nombre = dto.Nombre.Trim();
        await ValidarNombreUnico(nombre, 0);

        var departamento = new Departamento
        {
            Nombre = nombre,
            Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
            Activo = true
        };

        _departamentoRepositorio.Agregar(departamento);
        await _unidadTrabajo.GuardarCambiosAsync();

        return ADto(departamento);
    }

    public async Task<DepartamentoDto> Editar(int idDepartamento, DepartamentoGuardarDto dto)
    {
        var departamento = await ObtenerParaModificar(idDepartamento);
        var nombre = dto.Nombre.Trim();
        await ValidarNombreUnico(nombre, idDepartamento);

        departamento.Nombre = nombre;
        departamento.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
        await _unidadTrabajo.GuardarCambiosAsync();

        return ADto(departamento);
    }

    public async Task Activar(int idDepartamento)
    {
        var departamento = await ObtenerParaModificar(idDepartamento);

        departamento.Activo = true;
        await _unidadTrabajo.GuardarCambiosAsync();
    }

    public async Task Desactivar(int idDepartamento)
    {
        var departamento = await ObtenerParaModificar(idDepartamento);

        departamento.Activo = false;
        await _unidadTrabajo.GuardarCambiosAsync();
    }

    private async Task<Departamento> ObtenerParaModificar(int idDepartamento)
    {
        return await _departamentoRepositorio.ObtenerParaModificar(idDepartamento)
            ?? throw new ReglaNegocioException("El departamento no existe.", 404);
    }

    private async Task ValidarNombreUnico(string nombre, int idExcluir)
    {
        if (await _departamentoRepositorio.ExisteNombre(nombre, idExcluir))
        {
            throw new ReglaNegocioException($"Ya existe un departamento con el nombre '{nombre}'.", 409);
        }
    }

    private static DepartamentoDto ADto(Departamento departamento)
    {
        return new DepartamentoDto
        {
            IdDepartamento = departamento.IdDepartamento,
            Nombre = departamento.Nombre,
            Descripcion = departamento.Descripcion,
            Activo = departamento.Activo
        };
    }
}
