using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Constantes;

namespace Planilla.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Rrhh}")]
public class PuestoController : ControllerBase
{
    private readonly IPuestoServicio _puestoServicio;

    public PuestoController(IPuestoServicio puestoServicio)
    {
        _puestoServicio = puestoServicio;
    }

    [HttpGet]
    public async Task<ActionResult<List<PuestoDto>>> ObtenerTodos(bool soloActivos = false)
    {
        return await _puestoServicio.ObtenerTodos(soloActivos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PuestoDto>> ObtenerPorId(int id)
    {
        return await _puestoServicio.ObtenerPorId(id);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<PuestoDto>> Crear(PuestoGuardarDto dto)
    {
        var creado = await _puestoServicio.Crear(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.IdPuesto }, creado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<PuestoDto>> Editar(int id, PuestoGuardarDto dto)
    {
        return await _puestoServicio.Editar(id, dto);
    }

    [HttpPut("{id:int}/activar")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Activar(int id)
    {
        await _puestoServicio.Activar(id);
        return NoContent();
    }

    [HttpPut("{id:int}/desactivar")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Desactivar(int id)
    {
        await _puestoServicio.Desactivar(id);
        return NoContent();
    }
}
