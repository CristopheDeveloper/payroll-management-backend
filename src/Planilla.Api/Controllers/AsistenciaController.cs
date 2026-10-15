using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Constantes;

namespace Planilla.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Rrhh}")]
public class AsistenciaController : ControllerBase
{
    private readonly IAsistenciaServicio _asistenciaServicio;

    public AsistenciaController(IAsistenciaServicio asistenciaServicio)
    {
        _asistenciaServicio = asistenciaServicio;
    }

    [HttpGet("dia")]
    public async Task<ActionResult<DiaAsistenciaDto>> ObtenerDia(DateOnly? fecha = null)
    {
        return await _asistenciaServicio.ObtenerDia(fecha);
    }

    [HttpPost("lote")]
    public async Task<ActionResult<DiaAsistenciaDto>> GuardarDia(LoteAsistenciaDto dto)
    {
        return await _asistenciaServicio.GuardarDia(dto);
    }

    [HttpGet("resumen")]
    public async Task<ActionResult<List<ResumenAsistenciaDto>>> ObtenerResumen(int? anio = null, int? mes = null)
    {
        return await _asistenciaServicio.ObtenerResumen(anio, mes);
    }

    [HttpGet("empleado/{idEmpleado:int}")]
    public async Task<ActionResult<List<AsistenciaDto>>> ObtenerPorEmpleado(int idEmpleado, int? anio = null, int? mes = null)
    {
        return await _asistenciaServicio.ObtenerPorEmpleado(idEmpleado, anio, mes);
    }

    [HttpPost("rango")]
    public async Task<ActionResult<List<AsistenciaDto>>> RegistrarRango(RangoAsistenciaDto dto)
    {
        return await _asistenciaServicio.RegistrarRango(dto);
    }

    [HttpGet("empleado/{idEmpleado:int}/vacaciones")]
    public async Task<ActionResult<VacacionesAnioDto>> ObtenerVacaciones(int idEmpleado, int? anio = null)
    {
        return await _asistenciaServicio.ObtenerVacaciones(idEmpleado, anio);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _asistenciaServicio.Eliminar(id);
        return NoContent();
    }
}
