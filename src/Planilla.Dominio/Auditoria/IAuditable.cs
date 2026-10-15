namespace Planilla.Dominio.Auditoria;

public interface IAuditable
{
    string UsuarioIng { get; set; }
    DateTime FechaIng { get; set; }
    string UsuarioAct { get; set; }
    DateTime? FechaAct { get; set; }
}
