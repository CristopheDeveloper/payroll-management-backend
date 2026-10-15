using Planilla.Aplicacion.DTOs;

namespace Planilla.Aplicacion.Interfaces;

public interface IAutenticacionServicio
{
    Task<LoginRespuestaDto> Login(LoginDto dto);
    Task<bool> SesionVigente(int idUsuario, string rol);
}
