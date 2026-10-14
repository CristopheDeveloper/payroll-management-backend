# Planilla - Backend

API del sistema de nómina mensual para el curso de Desarrollo Web (UMG). Maneja empleados, puestos, asistencias y el cálculo de la nómina. El frontend está hecho en Vue y consume esta API.

Está hecha con ASP.NET Core Web API en .NET 10 y SQL Server.

## Estructura

La solución está dividida en capas:

- `src/Planilla.Api`: controllers, configuración y arranque de la aplicación.
- `src/Planilla.Aplicacion`: servicios con la lógica de negocio, interfaces y DTOs.
- `src/Planilla.Dominio`: entidades (se generan con scaffold, no se editan a mano).
- `src/Planilla.Infraestructura`: DbContext y repositorios.
- `tests/`: pruebas unitarias.
- `database/`: scripts SQL de la base de datos.

El flujo de una petición es Controller → Servicio → Repositorio → base de datos.

### Guardar cambios (Unidad de Trabajo)

Los repositorios no guardan: solo agregan, modifican o consultan. El servicio guarda una sola vez al final con `IUnidadTrabajo`, así todo lo que hizo con uno o varios repositorios queda en una sola transacción (se guarda todo o nada).

```csharp
_empleadoRepositorio.Agregar(empleado);
_otroRepositorio.Actualizar(otro);
await _unidadTrabajo.GuardarCambiosAsync();
```

Reglas:

- Los repositorios **nunca** llaman a `SaveChangesAsync`. El único lugar donde se guarda es `UnidadTrabajo`.
- Cada método de servicio que modifica datos termina con **un** `GuardarCambiosAsync()`.
- Si el servicio va a modificar una entidad, el repositorio la devuelve **sin** `AsNoTracking()`; si no, EF Core no detecta el cambio y no se guarda nada. Los listados y consultas de solo lectura sí usan `AsNoTracking()`.
- No uses `ExecuteUpdateAsync` ni `ExecuteDeleteAsync`, porque no pasan por `GuardarCambiosAsync()`. La única excepción es actualizar `UltimoAcceso` en el login.

### Campos de auditoría

Todas las tablas (excepto Bitacora) tienen `UsuarioIng`, `FechaIng`, `UsuarioAct` y `FechaAct`. Los llena `GuardarCambiosAsync()` con el usuario del token (o `SISTEMA` si no hay token); los servicios nunca los asignan.

Para que una entidad se audite tiene que implementar `IAuditable`. Como las entidades las genera el scaffold, eso se hace en `src/Planilla.Dominio/Auditoria/EntidadesAuditables.cs`: cada tabla nueva se agrega ahí con una línea, por ejemplo `public partial class Empleado : IAuditable { }`.

### Bitácora

`GuardarCambiosAsync()` también registra en la tabla `Bitacora` cada INSERT, UPDATE y DELETE de cualquier tabla, con los valores en JSON, el usuario y la fecha. No hay que hacer nada en los servicios, y todas las tablas se registran automáticamente (no hace falta agregarlas a ninguna lista).

- INSERT guarda todos los valores; DELETE, todos los valores anteriores; UPDATE, solo los campos que cambiaron.
- Los campos de auditoría no cuentan como cambios.
- El cambio y su registro en la bitácora se guardan en la misma transacción.
- Para que el UPDATE tenga los valores anteriores, la entidad tiene que haberse leído con seguimiento (la regla del `AsNoTracking()` de arriba). Nunca uses `Update()` con un objeto que viene del frontend.
- Las llaves foráneas se crean sin `ON DELETE CASCADE`: un borrado en cascada de la base no pasa por EF Core y no quedaría en la bitácora. Si hay que borrar hijos, el servicio los carga y los elimina con EF Core.

## Lo que necesitas instalar

- SDK de .NET 10
- SQL Server (Express o Developer sirve) o acceso al servidor del grupo
- Visual Studio 2026 o VS Code
- Si vas a hacer scaffold: `dotnet tool install -g dotnet-ef`

## Cómo levantarlo

### 1. Base de datos

Si vas a trabajar con tu SQL Server local, crea una base llamada `Payroll_Dev` y ejecuta los scripts de la carpeta `database/` en orden (por el número del nombre).

Si te conectas al servidor del grupo no tienes que crear nada, solo pide tu usuario.

### 2. Cadena de conexión

Los `appsettings` tienen `TU_USUARIO` y `TU_CONTRASEÑA` a propósito, para no subir credenciales al repo. **No los cambies ahí.** Cada quien pone su conexión en sus User Secrets.

Desde Visual Studio: clic derecho en `Planilla.Api` → Administrar secretos de usuario, y pega esto con tus datos:

```json
{
  "ConnectionStrings:DefaultConnection": "Server=db.notcodefirst.com,1433;Database=Payroll_Dev;User Id=tu_usuario;Password=tu_contraseña;TrustServerCertificate=True",
  "Jwt:Clave": "la clave del grupo"
}
```

Si usas tu SQL Server local con autenticación de Windows:

```json
{
  "ConnectionStrings:DefaultConnection": "Server=.\\SQLEXPRESS;Database=Payroll_Dev;Trusted_Connection=True;TrustServerCertificate=True",
  "Jwt:Clave": "la clave del grupo"
}
```

`Jwt:Clave` es la clave con la que se firman los tokens del login. Tiene que tener al menos 32 caracteres y, si no está, la API no arranca. Pídela al grupo para que todos usemos la misma.

También se puede por consola desde la raíz del proyecto:

```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "tu cadena de conexión" --project src/Planilla.Api
```

Los User Secrets solo se leen en Development.

### 3. Ejecutar

Abre `Planilla.slnx`, elige el perfil `https` y dale F5. Se abre Swagger en `https://localhost:7189/swagger`.

Por consola:

```
dotnet run --project src/Planilla.Api --launch-profile https
```

Para probar que todo está bien, ejecuta `POST /api/Autenticacion/login` en Swagger con el usuario inicial `admin`. Debería devolver un token. Cópialo, presiona **Authorize** en Swagger y pégalo; desde ahí Swagger lo manda en todas las peticiones a endpoints protegidos.

Los roles (`ADMIN` y `RRHH`) solo existen en la base de datos: los crea `01_Rol.sql` y no tienen mantenimiento. Solo se consultan con `GET /api/Usuario/roles` para asignarlos a un usuario. En el código sus nombres están como constantes en `Planilla.Dominio/Constantes/Roles.cs`; usa siempre `[Authorize(Roles = Roles.Admin)]` y nunca el texto suelto.

Sobre la seguridad:

- **Todos los endpoints piden token por defecto.** No hace falta poner `[Authorize]` para protegerlos; un endpoint solo queda público si lleva `[AllowAnonymous]`, y el login es el único que lo tiene. Para limitarlo a un rol usa `[Authorize(Roles = Roles.Admin)]`; para varios, `[Authorize(Roles = $"{Roles.Admin},{Roles.Rrhh}")]`.
- **En cada petición se revisa que el usuario del token siga activo y con el mismo rol.** Si el administrador lo desactiva o le cambia el rol, su token deja de servir de inmediato y tiene que volver a iniciar sesión.

## Ambientes

| Perfil | Ambiente | Base de datos |
|---|---|---|
| http / https | Development | Payroll_Dev |
| QA | QA | Payroll_QA |
| Production | Production | Payroll_Prod |

El perfil se cambia en la flecha junto al botón de ejecutar en Visual Studio. Swagger solo está disponible en Development.

Ojo con el perfil `Production` en local, porque apunta a la base real.

## Cambios en la base de datos

Trabajamos con Database First: primero se cambia la base y luego se generan las clases.

1. Crea un script nuevo en `database/` con el siguiente número. Nunca modifiques un script que ya se ejecutó en QA o en producción; si hay que corregir algo, se hace con otro script.
2. Ejecútalo en tu base.
3. Regenera las entidades desde la raíz del proyecto:

```
dotnet ef dbcontext scaffold "Name=ConnectionStrings:DefaultConnection" Microsoft.EntityFrameworkCore.SqlServer --project src/Planilla.Infraestructura --startup-project src/Planilla.Api --output-dir ../Planilla.Dominio/Entidades --namespace Planilla.Dominio.Entidades --context-dir AccesoDatos --context PlanillaDbContext --context-namespace Planilla.Infraestructura.AccesoDatos --data-annotations --no-onconfiguring --no-pluralize --force
```

Las tablas se nombran en singular (`Empleado`, `Puesto`) y su llave primaria es `Id<Tabla>` (`IdEmpleado`, `IdPuesto`). Las llaves foráneas se llaman igual que la llave a la que apuntan y se crean sin `ON DELETE CASCADE`. El `--no-pluralize` es necesario porque si no EF cambia algunos nombres en español (por ejemplo `Asistencia` queda como `Asistencium`).

No edites los archivos generados en `Entidades/` ni `PlanillaDbContext.cs`, porque se sobrescriben cada vez que se corre el scaffold.

## Errores de la API

Las validaciones de los datos de entrada devuelven 400 con el detalle de cada campo.

Para las reglas de negocio, los servicios lanzan `ReglaNegocioException`. Por defecto responde 400, pero se le puede pasar otro código:

```csharp
throw new ReglaNegocioException("Ya existe un empleado con ese DPI.", 409);
```

Cualquier otro error devuelve 500 con un mensaje genérico y queda registrado en el log.
