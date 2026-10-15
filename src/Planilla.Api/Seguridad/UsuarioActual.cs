using Planilla.Aplicacion.Interfaces;

namespace Planilla.Api.Seguridad;

public class UsuarioActual : IUsuarioActual
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioActual(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string NombreUsuario => _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "SISTEMA";
}
