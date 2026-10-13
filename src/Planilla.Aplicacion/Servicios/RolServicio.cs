using Planilla.Aplicacion.Excepciones;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Servicios;

public class RolServicio : IRolServicio
{
    private readonly IRolRepositorio _rolRepositorio;

    public RolServicio(IRolRepositorio rolRepositorio)
    {
        _rolRepositorio = rolRepositorio;
    }

    public async Task<List<Rol>> ObtenerTodos()
    {
        return await _rolRepositorio.ObtenerTodos();
    }

    public async Task<Rol> ObtenerPorId(int id)
    {
        return await _rolRepositorio.ObtenerPorId(id);
    }

    public async Task<Rol> Crear(Rol rol)
    {
        if (await _rolRepositorio.ExisteNombre(rol.Nombre))
        {
            throw new ReglaNegocioException($"Ya existe un rol con el nombre '{rol.Nombre}'.");
        }

        await _rolRepositorio.Agregar(rol);
        return rol;
    }
}
