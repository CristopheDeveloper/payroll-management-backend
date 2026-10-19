using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Constantes;

namespace Planilla.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Rrhh}")]
public class EmpleadoController : ControllerBase
{
    private readonly IEmpleadoServicio _empleadoServicio;

    public EmpleadoController(IEmpleadoServicio empleadoServicio)
    {
        _empleadoServicio = empleadoServicio;
    }

    [HttpGet]
    public async Task<ActionResult<List<EmpleadoDto>>> ObtenerTodos(bool? activo = null, int? idDepartamento = null,
        int? idPuesto = null, string buscar = null)
    {
        return await _empleadoServicio.ObtenerTodos(activo, idDepartamento, idPuesto, buscar);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmpleadoDetalleDto>> ObtenerPorId(int id)
    {
        return await _empleadoServicio.ObtenerPorId(id);
    }

    [HttpPost]
    public async Task<ActionResult<EmpleadoDetalleDto>> Contratar(EmpleadoCrearDto dto)
    {
        var creado = await _empleadoServicio.Contratar(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.IdEmpleado }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EmpleadoDetalleDto>> Editar(int id, EmpleadoEditarDto dto)
    {
        return await _empleadoServicio.Editar(id, dto);
    }

    [HttpPost("{id:int}/baja")]
    public async Task<ActionResult<EmpleadoDetalleDto>> DarDeBaja(int id, BajaDto dto)
    {
        return await _empleadoServicio.DarDeBaja(id, dto);
    }

    [HttpPost("{id:int}/reingreso")]
    public async Task<ActionResult<EmpleadoDetalleDto>> Reingresar(int id, ReingresoDto dto)
    {
        return await _empleadoServicio.Reingresar(id, dto);
    }

    [HttpPut("{id:int}/periodo-actual/fecha-ingreso")]
    public async Task<ActionResult<EmpleadoDetalleDto>> CorregirFechaIngreso(int id, CorregirFechaIngresoDto dto)
    {
        return await _empleadoServicio.CorregirFechaIngreso(id, dto);
    }
}
