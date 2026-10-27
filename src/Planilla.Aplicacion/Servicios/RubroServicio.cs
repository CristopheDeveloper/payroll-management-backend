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

    public async Task<List<RubroDto>> ObtenerTodos(bool soloActivos, string tipo, string origen)
    {
        if (tipo != null && tipo != "INGRESO" && tipo != "DESCUENTO" && tipo != "PATRONAL")
        {
            throw new ReglaNegocioException("El tipo debe ser INGRESO, DESCUENTO o PATRONAL.");
        }
        if (origen != null && origen != "SISTEMA" && origen != "MANUAL")
        {
            throw new ReglaNegocioException("El origen debe ser SISTEMA o MANUAL.");
        }

        var rubros = await _rubroRepositorio.ObtenerTodos(soloActivos, tipo, origen);
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
        var codigo = dto.Codigo.Trim();
        var nombre = dto.Nombre.Trim();
        if (await _rubroRepositorio.ExisteCodigo(codigo))
        {
            throw new ReglaNegocioException($"Ya existe un rubro con el código '{codigo}'.", 409);
        }
        if (await _rubroRepositorio.ExisteNombre(nombre, null))
        {
            throw new ReglaNegocioException($"Ya existe un rubro con el nombre '{nombre}'.", 409);
        }
        if (dto.Tipo == "DESCUENTO" && dto.AfectoIgss)
        {
            throw new ReglaNegocioException("Solo un rubro de ingreso puede ser afecto al IGSS.");
        }

        var rubro = new Rubro
        {
            Codigo = codigo,
            Nombre = nombre,
            Tipo = dto.Tipo,
            Origen = "MANUAL",
            AfectoIgss = dto.AfectoIgss,
            Orden = dto.Orden,
            Descripcion = Texto(dto.Descripcion),
            CuentaContable = Texto(dto.CuentaContable),
            Activo = true
        };

        _rubroRepositorio.Agregar(rubro);
        await _unidadTrabajo.GuardarCambiosAsync();

        return ADto(rubro);
    }

    public async Task<RubroDto> Editar(int idRubro, RubroEditarDto dto)
    {
        var rubro = await ObtenerManualParaModificar(idRubro);
        var nombre = dto.Nombre.Trim();
        if (await _rubroRepositorio.ExisteNombre(nombre, idRubro))
        {
            throw new ReglaNegocioException($"Ya existe un rubro con el nombre '{nombre}'.", 409);
        }
        if (rubro.Tipo == "DESCUENTO" && dto.AfectoIgss)
        {
            throw new ReglaNegocioException("Solo un rubro de ingreso puede ser afecto al IGSS.");
        }

        rubro.Nombre = nombre;
        rubro.AfectoIgss = dto.AfectoIgss;
        rubro.Orden = dto.Orden;
        rubro.Descripcion = Texto(dto.Descripcion);
        rubro.CuentaContable = Texto(dto.CuentaContable);

        await _unidadTrabajo.GuardarCambiosAsync();

        return ADto(rubro);
    }

    public async Task CambiarEstado(int idRubro, bool activo)
    {
        var rubro = await ObtenerManualParaModificar(idRubro);
        if (rubro.Activo == activo)
        {
            throw new ReglaNegocioException(activo ? "El rubro ya está activo." : "El rubro ya está inactivo.", 409);
        }

        rubro.Activo = activo;
        await _unidadTrabajo.GuardarCambiosAsync();
    }

    private async Task<Rubro> ObtenerManualParaModificar(int idRubro)
    {
        var rubro = await _rubroRepositorio.ObtenerParaModificar(idRubro)
            ?? throw new ReglaNegocioException("El rubro no existe.", 404);
        if (rubro.Origen == "SISTEMA")
        {
            throw new ReglaNegocioException("Los rubros del sistema no se pueden modificar.", 409);
        }

        return rubro;
    }

    private static string Texto(string valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }

    private static RubroDto ADto(Rubro rubro)
    {
        return new RubroDto
        {
            IdRubro = rubro.IdRubro,
            Codigo = rubro.Codigo,
            Nombre = rubro.Nombre,
            Tipo = rubro.Tipo,
            Origen = rubro.Origen,
            AfectoIgss = rubro.AfectoIgss,
            EsPrestacion = rubro.EsPrestacion,
            Orden = rubro.Orden,
            Descripcion = rubro.Descripcion,
            CuentaContable = rubro.CuentaContable,
            Activo = rubro.Activo
        };
    }
}
