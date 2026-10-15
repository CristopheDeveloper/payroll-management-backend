using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Constantes;

namespace Planilla.Api.Controllers;

[ApiController]
[Route("api/Nomina/{idNomina:int}/movimientos")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Rrhh}")]
public class MovimientoNominaController : ControllerBase
{
    private readonly IMovimientoNominaServicio _movimientoServicio;

    public MovimientoNominaController(IMovimientoNominaServicio movimientoServicio)
    {
        _movimientoServicio = movimientoServicio;
    }

    [HttpGet]
    public async Task<ActionResult<List<MovimientoDto>>> ObtenerTodos(int idNomina, int? idEmpleado = null)
    {
        return await _movimientoServicio.ObtenerTodos(idNomina, idEmpleado);
    }

    [HttpPost]
    public async Task<ActionResult<MovimientoDto>> Crear(int idNomina, MovimientoGuardarDto dto)
    {
        return await _movimientoServicio.Crear(idNomina, dto);
    }

    [HttpPut("{idMovimiento:int}")]
    public async Task<ActionResult<MovimientoDto>> Editar(int idNomina, int idMovimiento, MovimientoGuardarDto dto)
    {
        return await _movimientoServicio.Editar(idNomina, idMovimiento, dto);
    }

    [HttpDelete("{idMovimiento:int}")]
    public async Task<IActionResult> Eliminar(int idNomina, int idMovimiento)
    {
        await _movimientoServicio.Eliminar(idNomina, idMovimiento);
        return NoContent();
    }
}
