using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Constantes;

namespace Planilla.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Rrhh}")]
public class RubroController : ControllerBase
{
    private readonly IRubroServicio _rubroServicio;

    public RubroController(IRubroServicio rubroServicio)
    {
        _rubroServicio = rubroServicio;
    }

    [HttpGet]
    public async Task<ActionResult<List<RubroDto>>> ObtenerTodos(bool soloActivos = false, string tipo = null, string origen = null)
    {
        return await _rubroServicio.ObtenerTodos(soloActivos, tipo, origen);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RubroDto>> ObtenerPorId(int id)
    {
        return await _rubroServicio.ObtenerPorId(id);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<RubroDto>> Crear(RubroCrearDto dto)
    {
        var creado = await _rubroServicio.Crear(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.IdRubro }, creado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<RubroDto>> Editar(int id, RubroEditarDto dto)
    {
        return await _rubroServicio.Editar(id, dto);
    }

    [HttpPut("{id:int}/activar")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Activar(int id)
    {
        await _rubroServicio.CambiarEstado(id, true);
        return NoContent();
    }

    [HttpPut("{id:int}/desactivar")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Desactivar(int id)
    {
        await _rubroServicio.CambiarEstado(id, false);
        return NoContent();
    }
}
