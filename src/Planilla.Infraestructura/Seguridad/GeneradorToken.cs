using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Planilla.Aplicacion.Interfaces;

namespace Planilla.Infraestructura.Seguridad;

public class GeneradorToken : IGeneradorToken
{
    private readonly SigningCredentials _credenciales;
    private readonly string _emisor;
    private readonly string _audiencia;
    private readonly int _duracionMinutos;

    public GeneradorToken(string clave, string emisor, string audiencia, int duracionMinutos)
    {
        var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave));
        _credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);
        _emisor = emisor;
        _audiencia = audiencia;
        _duracionMinutos = duracionMinutos;
    }

    public string Generar(int usuarioId, string nombreUsuario, string rol)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("usuarioId", usuarioId.ToString()),
                new Claim("usuario", nombreUsuario),
                new Claim("rol", rol)
            }),
            Issuer = _emisor,
            Audience = _audiencia,
            Expires = DateTime.UtcNow.AddMinutes(_duracionMinutos),
            SigningCredentials = _credenciales
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
