namespace Planilla.Aplicacion.Excepciones;

public class ReglaNegocioException : Exception
{
    public int CodigoHttp { get; }

    public ReglaNegocioException(string mensaje, int codigoHttp = 400) : base(mensaje)
    {
        CodigoHttp = codigoHttp;
    }
}
