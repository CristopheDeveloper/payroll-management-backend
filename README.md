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

Para probar que todo está bien, ejecuta `GET /api/Rol` en Swagger. Debería devolver los roles Admin y RRHH.

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

Las tablas se nombran en singular (`Empleado`, `Puesto`). El `--no-pluralize` es necesario porque si no EF cambia algunos nombres en español (por ejemplo `Asistencia` queda como `Asistencium`).

No edites los archivos generados en `Entidades/` ni `PlanillaDbContext.cs`, porque se sobrescriben cada vez que se corre el scaffold.

## Errores de la API

Las validaciones de los datos de entrada devuelven 400 con el detalle de cada campo.

Para las reglas de negocio, los servicios lanzan `ReglaNegocioException`. Por defecto responde 400, pero se le puede pasar otro código:

```csharp
throw new ReglaNegocioException("Ya existe un empleado con ese DPI.", 409);
```

Cualquier otro error devuelve 500 con un mensaje genérico y queda registrado en el log.
