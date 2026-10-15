using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Constantes;

namespace Planilla.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Rrhh}")]
public class DepartamentoController : ControllerBase
{
    private readonly IDepartamentoServicio _departamentoServicio;

    public DepartamentoController(IDepartamentoServicio departamentoServicio)
    {
        _departamentoServicio = departamentoServicio;
    }

    [HttpGet]
    public async Task<ActionResult<List<DepartamentoDto>>> ObtenerTodos(bool soloActivos = false)
    {
        return await _departamentoServicio.ObtenerTodos(soloActivos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartamentoDto>> ObtenerPorId(int id)
    {
        return await _departamentoServicio.ObtenerPorId(id);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<DepartamentoDto>> Crear(DepartamentoGuardarDto dto)
    {
        var creado = await _departamentoServicio.Crear(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.IdDepartamento }, creado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<DepartamentoDto>> Editar(int id, DepartamentoGuardarDto dto)
    {
        return await _departamentoServicio.Editar(id, dto);
    }

    [HttpPut("{id:int}/activar")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Activar(int id)
    {
        await _departamentoServicio.Activar(id);
        return NoContent();
    }

    [HttpPut("{id:int}/desactivar")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Desactivar(int id)
    {
        await _departamentoServicio.Desactivar(id);
        return NoContent();
    }
}
