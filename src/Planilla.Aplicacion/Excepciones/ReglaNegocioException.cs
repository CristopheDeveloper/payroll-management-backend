namespace Planilla.Aplicacion.Excepciones;

public class ReglaNegocioException : Exception
{
    public int CodigoHttp { get; }
    public string Codigo { get; }

    public ReglaNegocioException(string mensaje, int codigoHttp = 400, string codigo = null) : base(mensaje)
    {
        CodigoHttp = codigoHttp;
        Codigo = codigo;
    }
}
