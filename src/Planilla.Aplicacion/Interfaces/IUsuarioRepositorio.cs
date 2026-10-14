using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Interfaces;

public interface IUsuarioRepositorio
{
    Task<List<Usuario>> ObtenerTodos();
    Task<Usuario> ObtenerPorId(int idUsuario);
    Task<Usuario> ObtenerParaModificar(int idUsuario);
    Task<Usuario> ObtenerPorNombreUsuario(string nombreUsuario);
    Task<bool> ExisteNombreUsuario(string nombreUsuario);
    Task<int> ContarActivosPorRol(string rol);
    Task<Rol> ObtenerRol(int idRol);
    Task<List<Rol>> ObtenerRolesActivos();
    void Agregar(Usuario usuario);
    Task ActualizarUltimoAcceso(int idUsuario);
}
