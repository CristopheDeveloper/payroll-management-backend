namespace Planilla.Aplicacion.Interfaces;

public interface IHashContrasena
{
    string Hashear(string contrasena);
    bool Verificar(string contrasena, string hash);
}
