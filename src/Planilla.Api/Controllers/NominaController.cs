using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Constantes;

namespace Planilla.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Rrhh}")]
public class NominaController : ControllerBase
{
    private readonly INominaServicio _nominaServicio;

    public NominaController(INominaServicio nominaServicio)
    {
        _nominaServicio = nominaServicio;
    }

    [HttpGet]
    public async Task<ActionResult<List<NominaDto>>> ObtenerTodas(int? anio = null)
    {
        return await _nominaServicio.ObtenerTodas(anio);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NominaDto>> ObtenerPorId(int id)
    {
        return await _nominaServicio.ObtenerPorId(id);
    }

    [HttpPost]
    public async Task<ActionResult<NominaDto>> Crear(NominaCrearDto dto)
    {
        var creada = await _nominaServicio.Crear(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.IdNomina }, creada);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _nominaServicio.Eliminar(id);
        return NoContent();
    }

    [HttpGet("{id:int}/empleados")]
    public async Task<ActionResult<List<EmpleadoMesDto>>> ObtenerEmpleados(int id)
    {
        return await _nominaServicio.ObtenerEmpleados(id);
    }

    [HttpGet("{id:int}/detalle")]
    public async Task<ActionResult<List<NominaDetalleDto>>> ObtenerDetalle(int id)
    {
        return await _nominaServicio.ObtenerDetalle(id);
    }

    [HttpGet("{id:int}/comparacion")]
    public async Task<ActionResult<ComparacionDto>> ObtenerComparacion(int id)
    {
        return await _nominaServicio.ObtenerComparacion(id);
    }

    [HttpPost("{id:int}/calcular")]
    public async Task<ActionResult<NominaDto>> Calcular(int id, CalcularDto dto)
    {
        return await _nominaServicio.Calcular(id, dto);
    }

    [HttpPost("{id:int}/cerrar")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<NominaDto>> Cerrar(int id, CerrarDto dto)
    {
        return await _nominaServicio.Cerrar(id, dto);
    }

    [HttpPost("{id:int}/reabrir")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<NominaDto>> Reabrir(int id, ReabrirDto dto)
    {
        return await _nominaServicio.Reabrir(id, dto);
    }
}
