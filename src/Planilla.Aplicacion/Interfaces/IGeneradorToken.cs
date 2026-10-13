namespace Planilla.Aplicacion.Interfaces;

public interface IGeneradorToken
{
    string Generar(int usuarioId, string nombreUsuario, string rol);
}
