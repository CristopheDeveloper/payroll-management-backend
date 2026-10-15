using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Constantes;

namespace Planilla.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioServicio _usuarioServicio;

    public UsuarioController(IUsuarioServicio usuarioServicio)
    {
        _usuarioServicio = usuarioServicio;
    }

    [HttpGet]
    public async Task<ActionResult<List<UsuarioDto>>> ObtenerTodos()
    {
        return await _usuarioServicio.ObtenerTodos();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> ObtenerPorId(int id)
    {
        return await _usuarioServicio.ObtenerPorId(id);
    }

    [HttpGet("roles")]
    public async Task<ActionResult<List<RolDto>>> ObtenerRoles()
    {
        return await _usuarioServicio.ObtenerRoles();
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Crear(UsuarioCrearDto dto)
    {
        var creado = await _usuarioServicio.Crear(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.IdUsuario }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> Editar(int id, UsuarioEditarDto dto)
    {
        return await _usuarioServicio.Editar(id, dto);
    }

    [HttpPut("{id:int}/contrasena")]
    public async Task<IActionResult> RestablecerContrasena(int id, RestablecerContrasenaDto dto)
    {
        await _usuarioServicio.RestablecerContrasena(id, dto);
        return NoContent();
    }

    [HttpPut("{id:int}/activar")]
    public async Task<IActionResult> Activar(int id)
    {
        await _usuarioServicio.Activar(id);
        return NoContent();
    }

    [HttpPut("{id:int}/desactivar")]
    public async Task<IActionResult> Desactivar(int id)
    {
        await _usuarioServicio.Desactivar(id);
        return NoContent();
    }
}
