using Planilla.Aplicacion.DTOs;

namespace Planilla.Aplicacion.Interfaces;

public interface IUsuarioServicio
{
    Task<List<UsuarioDto>> ObtenerTodos();
    Task<UsuarioDto> ObtenerPorId(int idUsuario);
    Task<List<RolDto>> ObtenerRoles();
    Task<UsuarioDto> Crear(UsuarioCrearDto dto);
    Task<UsuarioDto> Editar(int idUsuario, UsuarioEditarDto dto);
    Task RestablecerContrasena(int idUsuario, RestablecerContrasenaDto dto);
    Task Activar(int idUsuario);
    Task Desactivar(int idUsuario);
}
