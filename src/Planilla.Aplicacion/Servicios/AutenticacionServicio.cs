using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Excepciones;
using Planilla.Aplicacion.Interfaces;

namespace Planilla.Aplicacion.Servicios;

public class AutenticacionServicio : IAutenticacionServicio
{
    private readonly IUsuarioRepositorio _usuarioRepositorio;
    private readonly IHashContrasena _hashContrasena;
    private readonly IGeneradorToken _generadorToken;

    public AutenticacionServicio(IUsuarioRepositorio usuarioRepositorio, IHashContrasena hashContrasena, IGeneradorToken generadorToken)
    {
        _usuarioRepositorio = usuarioRepositorio;
        _hashContrasena = hashContrasena;
        _generadorToken = generadorToken;
    }

    public async Task<LoginRespuestaDto> Login(LoginDto dto)
    {
        var usuario = await _usuarioRepositorio.ObtenerPorNombreUsuario(dto.NombreUsuario.Trim());

        if (usuario == null
            || !usuario.Activo
            || !usuario.IdRolNavigation.Activo
            || !_hashContrasena.Verificar(dto.Contrasena, usuario.PasswordHash))
        {
            throw new ReglaNegocioException("Usuario o contraseña incorrectos.", 401);
        }

        await _usuarioRepositorio.ActualizarUltimoAcceso(usuario.IdUsuario);

        var rol = usuario.IdRolNavigation.Nombre;
        return new LoginRespuestaDto
        {
            Token = _generadorToken.Generar(usuario.IdUsuario, usuario.NombreUsuario, rol),
            NombreUsuario = usuario.NombreUsuario,
            NombreCompleto = usuario.NombreCompleto,
            Rol = rol
        };
    }

    public async Task<bool> SesionVigente(int idUsuario, string rol)
    {
        var usuario = await _usuarioRepositorio.ObtenerPorId(idUsuario);

        return usuario != null
            && usuario.Activo
            && usuario.IdRolNavigation.Activo
            && usuario.IdRolNavigation.Nombre == rol;
    }
}
