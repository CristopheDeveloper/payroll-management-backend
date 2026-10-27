using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Planilla.Api.Seguridad;
using Planilla.Aplicacion.Excepciones;
using Planilla.Aplicacion.Interfaces;
using Planilla.Aplicacion.Servicios;
using Planilla.Infraestructura.AccesoDatos;
using Planilla.Infraestructura.AccesoDatos.Repositorios;
using Planilla.Infraestructura.Reportes;
using Planilla.Infraestructura.Seguridad;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PlanillaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUnidadTrabajo, UnidadTrabajo>();
builder.Services.AddScoped<Auditoria>();
builder.Services.AddScoped<RegistroBitacora>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioActual, UsuarioActual>();

builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IDepartamentoRepositorio, DepartamentoRepositorio>();
builder.Services.AddScoped<IPuestoRepositorio, PuestoRepositorio>();
builder.Services.AddScoped<IParametroRepositorio, ParametroRepositorio>();
builder.Services.AddScoped<IEmpleadoRepositorio, EmpleadoRepositorio>();
builder.Services.AddScoped<IAsistenciaRepositorio, AsistenciaRepositorio>();
builder.Services.AddScoped<IRubroRepositorio, RubroRepositorio>();
builder.Services.AddScoped<INominaRepositorio, NominaRepositorio>();

builder.Services.AddScoped<IAutenticacionServicio, AutenticacionServicio>();
builder.Services.AddScoped<IUsuarioServicio, UsuarioServicio>();
builder.Services.AddScoped<IDepartamentoServicio, DepartamentoServicio>();
builder.Services.AddScoped<IPuestoServicio, PuestoServicio>();
builder.Services.AddScoped<IParametroServicio, ParametroServicio>();
builder.Services.AddScoped<IEmpleadoServicio, EmpleadoServicio>();
builder.Services.AddScoped<IAsistenciaServicio, AsistenciaServicio>();
builder.Services.AddScoped<IRubroServicio, RubroServicio>();
builder.Services.AddScoped<INominaServicio, NominaServicio>();
builder.Services.AddScoped<IReporteServicio, ReporteServicio>();

QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.ThrowOnMissingTextGlyphs = false;
builder.Services.AddSingleton<IGeneradorDocumentos, GeneradorDocumentos>();

var claveJwt = builder.Configuration["Jwt:Clave"];
if (string.IsNullOrWhiteSpace(claveJwt) || claveJwt.Length < 32)
{
    throw new InvalidOperationException("Falta configurar Jwt:Clave (mínimo 32 caracteres). En desarrollo va en User Secrets.");
}
var emisorJwt = builder.Configuration["Jwt:Emisor"];
var audienciaJwt = builder.Configuration["Jwt:Audiencia"];
var duracionJwt = builder.Configuration.GetValue<int>("Jwt:DuracionMinutos");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = emisorJwt,
            ValidateAudience = true,
            ValidAudience = audienciaJwt,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveJwt)),
            NameClaimType = "usuario",
            RoleClaimType = "rol"
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var idUsuario = context.Principal?.FindFirst("usuarioId")?.Value;
                var rol = context.Principal?.FindFirst("rol")?.Value;
                var autenticacion = context.HttpContext.RequestServices.GetRequiredService<IAutenticacionServicio>();

                if (!int.TryParse(idUsuario, out var id) || !await autenticacion.SesionVigente(id, rol))
                {
                    context.Fail("La sesión ya no es válida.");
                }
            }
        };
    });

builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddSingleton<IGeneradorToken>(new GeneradorToken(claveJwt, emisorJwt, audienciaJwt, duracionJwt));
builder.Services.AddSingleton<IHashContrasena, HashContrasena>();

var origenesPermitidos = builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(origenesPermitidos)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("Content-Disposition"));
});

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Token JWT obtenido en el login"
        };
        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    SuppressDiagnosticsCallback = contexto => true,
    ExceptionHandler = async context =>
    {
        var excepcion = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (excepcion is ReglaNegocioException reglaNegocio)
        {
            await Results.Problem(
                title: "Regla de negocio",
                detail: reglaNegocio.Message,
                statusCode: reglaNegocio.CodigoHttp,
                extensions: reglaNegocio.Codigo == null ? null : new Dictionary<string, object> { ["codigo"] = reglaNegocio.Codigo }
            ).ExecuteAsync(context);
            return;
        }

        if (excepcion is DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } })
        {
            await Results.Problem(
                title: "Regla de negocio",
                detail: "Ya existe un registro con ese valor.",
                statusCode: StatusCodes.Status409Conflict
            ).ExecuteAsync(context);
            return;
        }

        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(excepcion, "Error no controlado en {Metodo} {Ruta}", context.Request.Method, context.Request.Path);

        await Results.Problem(
            title: "Ocurrió un error inesperado.",
            detail: app.Environment.IsDevelopment() ? excepcion?.Message : null,
            statusCode: StatusCodes.Status500InternalServerError
        ).ExecuteAsync(context);
    }
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Planilla API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
