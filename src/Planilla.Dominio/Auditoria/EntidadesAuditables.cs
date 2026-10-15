using Planilla.Dominio.Auditoria;

namespace Planilla.Dominio.Entidades;

// Cada tabla nueva con campos de auditoría se agrega a esta lista.
public partial class Rol : IAuditable { }

public partial class Usuario : IAuditable { }

public partial class Departamento : IAuditable { }

public partial class Puesto : IAuditable { }
