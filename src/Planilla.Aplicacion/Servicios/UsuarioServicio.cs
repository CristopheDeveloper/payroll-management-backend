using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Excepciones;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Constantes;
using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Servicios;

public class UsuarioServicio : IUsuarioServicio
{
    private readonly IUsuarioRepositorio _usuarioRepositorio;
    private readonly IUnidadTrabajo _unidadTrabajo;
    private readonly IHashContrasena _hashContrasena;
    private readonly IUsuarioActual _usuarioActual;

    public UsuarioServicio(IUsuarioRepositorio usuarioRepositorio, IUnidadTrabajo unidadTrabajo,
        IHashContrasena hashContrasena, IUsuarioActual usuarioActual)
    {
        _usuarioRepositorio = usuarioRepositorio;
        _unidadTrabajo = unidadTrabajo;
        _hashContrasena = hashContrasena;
        _usuarioActual = usuarioActual;
    }

    public async Task<List<UsuarioDto>> ObtenerTodos()
    {
        var usuarios = await _usuarioRepositorio.ObtenerTodos();
        return usuarios.Select(u => ADto(u, u.IdRolNavigation.Nombre)).ToList();
    }

    public async Task<UsuarioDto> ObtenerPorId(int idUsuario)
    {
        var usuario = await _usuarioRepositorio.ObtenerPorId(idUsuario)
            ?? throw new ReglaNegocioException("El usuario no existe.", 404);

        return ADto(usuario, usuario.IdRolNavigation.Nombre);
    }

    public async Task<List<RolDto>> ObtenerRoles()
    {
        var roles = await _usuarioRepositorio.ObtenerRolesActivos();
        return roles.Select(r => new RolDto
        {
            IdRol = r.IdRol,
            Nombre = r.Nombre,
            Descripcion = r.Descripcion
        }).ToList();
    }

    public async Task<UsuarioDto> Crear(UsuarioCrearDto dto)
    {
        var nombreUsuario = dto.NombreUsuario.Trim();
        if (await _usuarioRepositorio.ExisteNombreUsuario(nombreUsuario))
        {
            throw new ReglaNegocioException($"Ya existe un usuario con el nombre '{nombreUsuario}'.", 409);
        }

        var rol = await ObtenerRolActivo(dto.IdRol);

        var usuario = new Usuario
        {
            NombreUsuario = nombreUsuario,
            NombreCompleto = dto.NombreCompleto.Trim(),
            IdRol = rol.IdRol,
            PasswordHash = _hashContrasena.Hashear(dto.Contrasena),
            Activo = true
        };

        _usuarioRepositorio.Agregar(usuario);
        await _unidadTrabajo.GuardarCambiosAsync();

        return ADto(usuario, rol.Nombre);
    }

    public async Task<UsuarioDto> Editar(int idUsuario, UsuarioEditarDto dto)
    {
        var usuario = await ObtenerParaModificar(idUsuario);
        var rolNuevo = await ObtenerRolActivo(dto.IdRol);

        if (usuario.IdRol != rolNuevo.IdRol)
        {
            var rolActual = await _usuarioRepositorio.ObtenerRol(usuario.IdRol);
            if (usuario.Activo && rolActual.Nombre == Roles.Admin)
            {
                await ValidarQueNoSeaElUltimoAdmin();
            }
        }

        usuario.NombreCompleto = dto.NombreCompleto.Trim();
        usuario.IdRol = rolNuevo.IdRol;
        await _unidadTrabajo.GuardarCambiosAsync();

        return ADto(usuario, rolNuevo.Nombre);
    }

    public async Task RestablecerContrasena(int idUsuario, RestablecerContrasenaDto dto)
    {
        var usuario = await ObtenerParaModificar(idUsuario);

        usuario.PasswordHash = _hashContrasena.Hashear(dto.Contrasena);
        await _unidadTrabajo.GuardarCambiosAsync();
    }

    public async Task Activar(int idUsuario)
    {
        var usuario = await ObtenerParaModificar(idUsuario);

        usuario.Activo = true;
        await _unidadTrabajo.GuardarCambiosAsync();
    }

    public async Task Desactivar(int idUsuario)
    {
        var usuario = await ObtenerParaModificar(idUsuario);
        if (!usuario.Activo)
        {
            return;
        }

        if (usuario.NombreUsuario == _usuarioActual.NombreUsuario)
        {
            throw new ReglaNegocioException("No puedes desactivar tu propio usuario.");
        }

        var rol = await _usuarioRepositorio.ObtenerRol(usuario.IdRol);
        if (rol.Nombre == Roles.Admin)
        {
            await ValidarQueNoSeaElUltimoAdmin();
        }

        usuario.Activo = false;
        await _unidadTrabajo.GuardarCambiosAsync();
    }

    private async Task<Usuario> ObtenerParaModificar(int idUsuario)
    {
        return await _usuarioRepositorio.ObtenerParaModificar(idUsuario)
            ?? throw new ReglaNegocioException("El usuario no existe.", 404);
    }

    private async Task<Rol> ObtenerRolActivo(int idRol)
    {
        var rol = await _usuarioRepositorio.ObtenerRol(idRol);
        if (rol == null || !rol.Activo)
        {
            throw new ReglaNegocioException("El rol no existe o está inactivo.");
        }
        return rol;
    }

    private async Task ValidarQueNoSeaElUltimoAdmin()
    {
        if (await _usuarioRepositorio.ContarActivosPorRol(Roles.Admin) <= 1)
        {
            throw new ReglaNegocioException("Debe existir al menos un administrador activo.");
        }
    }

    private static UsuarioDto ADto(Usuario usuario, string rol)
    {
        return new UsuarioDto
        {
            IdUsuario = usuario.IdUsuario,
            NombreUsuario = usuario.NombreUsuario,
            NombreCompleto = usuario.NombreCompleto,
            IdRol = usuario.IdRol,
            Rol = rol,
            Activo = usuario.Activo,
            UltimoAcceso = usuario.UltimoAcceso
        };
    }
}
