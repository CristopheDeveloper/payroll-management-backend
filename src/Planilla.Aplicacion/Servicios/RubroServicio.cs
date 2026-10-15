using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Excepciones;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Servicios;

public class RubroServicio : IRubroServicio
{
    private readonly IRubroRepositorio _rubroRepositorio;
    private readonly IUnidadTrabajo _unidadTrabajo;

    public RubroServicio(IRubroRepositorio rubroRepositorio, IUnidadTrabajo unidadTrabajo)
    {
        _rubroRepositorio = rubroRepositorio;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<List<RubroDto>> ObtenerTodos(bool soloActivos, string tipo)
    {
        if (tipo != null && tipo != "INGRESO" && tipo != "DESCUENTO")
        {
            throw new ReglaNegocioException("El tipo debe ser INGRESO o DESCUENTO.");
        }

        var rubros = await _rubroRepositorio.ObtenerTodos(soloActivos, tipo);
        return rubros.Select(ADto).ToList();
    }

    public async Task<RubroDto> ObtenerPorId(int idRubro)
    {
        var rubro = await _rubroRepositorio.ObtenerPorId(idRubro)
            ?? throw new ReglaNegocioException("El rubro no existe.", 404);

        return ADto(rubro);
    }

    public async Task<RubroDto> Crear(RubroCrearDto dto)
    {
        var nombre = dto.Nombre.Trim();
        if (await _rubroRepositorio.ExisteNombre(nombre))
        {
            throw new ReglaNegocioException($"Ya existe un rubro con el nombre '{nombre}'.", 409);
        }
        if (dto.Tipo == "DESCUENTO" && dto.AfectoIgss)
        {
            throw new ReglaNegocioException("Solo un rubro de ingreso puede ser afecto al IGSS.");
        }

        var rubro = new Rubro
        {
            Nombre = nombre,
            Tipo = dto.Tipo,
            AfectoIgss = dto.AfectoIgss,
            Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
            CuentaContable = string.IsNullOrWhiteSpace(dto.CuentaContable) ? null : dto.CuentaContable.Trim(),
            Activo = true
        };

        _rubroRepositorio.Agregar(rubro);
        await _unidadTrabajo.GuardarCambiosAsync();

        return ADto(rubro);
    }

    private static RubroDto ADto(Rubro rubro)
    {
        return new RubroDto
        {
            IdRubro = rubro.IdRubro,
            Nombre = rubro.Nombre,
            Tipo = rubro.Tipo,
            AfectoIgss = rubro.AfectoIgss,
            Descripcion = rubro.Descripcion,
            CuentaContable = rubro.CuentaContable,
            Activo = rubro.Activo
        };
    }
}
