using Planilla.Aplicacion.Interfaces;

namespace Planilla.Infraestructura.Seguridad;

public class HashContrasena : IHashContrasena
{
    public string Hashear(string contrasena)
    {
        return BCrypt.Net.BCrypt.HashPassword(contrasena);
    }

    public bool Verificar(string contrasena, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(contrasena, hash);
    }
}
