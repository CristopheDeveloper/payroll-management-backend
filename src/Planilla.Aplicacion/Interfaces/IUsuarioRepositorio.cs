using Planilla.Dominio.Entidades;

namespace Planilla.Aplicacion.Interfaces;

public interface IUsuarioRepositorio
{
    Task<Usuario> ObtenerPorId(int idUsuario);
    Task<Usuario> ObtenerPorNombreUsuario(string nombreUsuario);
    Task ActualizarUltimoAcceso(int idUsuario);
}
