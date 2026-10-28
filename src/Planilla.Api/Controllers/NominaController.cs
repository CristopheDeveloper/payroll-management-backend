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

    [HttpGet("tipos")]
    public async Task<ActionResult<List<TipoPlanillaDto>>> ObtenerTiposPlanilla(bool soloActivos = false)
    {
        return await _nominaServicio.ObtenerTiposPlanilla(soloActivos);
    }

    [HttpGet]
    public async Task<ActionResult<List<NominaDto>>> ObtenerTodas(int? anio = null, int? idTipoPlanilla = null)
    {
        return await _nominaServicio.ObtenerTodas(anio, idTipoPlanilla);
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

    [HttpPut("{id:int}")]
    public async Task<ActionResult<NominaDto>> Editar(int id, NominaEditarDto dto)
    {
        return await _nominaServicio.Editar(id, dto);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _nominaServicio.Eliminar(id);
        return NoContent();
    }

    [HttpPost("{id:int}/preparar")]
    public async Task<ActionResult<NominaDto>> Preparar(int id)
    {
        return await _nominaServicio.Preparar(id);
    }

    [HttpGet("{id:int}/empleados")]
    public async Task<ActionResult<List<NominaEmpleadoDto>>> ObtenerEmpleados(int id)
    {
        return await _nominaServicio.ObtenerEmpleados(id);
    }

    [HttpPut("{id:int}/empleados/{idEmpleado:int}")]
    public async Task<ActionResult<NominaEmpleadoDto>> GuardarDatosEmpleado(int id, int idEmpleado, DatosEmpleadoGuardarDto dto)
    {
        return await _nominaServicio.GuardarDatosEmpleado(id, idEmpleado, dto);
    }

    [HttpPut("{id:int}/variables")]
    public async Task<ActionResult<List<NominaEmpleadoDto>>> GuardarVariable(int id, VariableGuardarDto dto)
    {
        return await _nominaServicio.GuardarVariable(id, dto);
    }

    [HttpPut("{id:int}/variables/importar")]
    public async Task<ActionResult<List<NominaEmpleadoDto>>> ImportarVariables(int id, ImportarVariablesDto dto)
    {
        return await _nominaServicio.GuardarVariables(id, dto);
    }

    [HttpPost("{id:int}/generar")]
    public async Task<ActionResult<NominaDto>> Generar(int id)
    {
        return await _nominaServicio.Generar(id);
    }

    [HttpPost("{id:int}/volver-a-preparada")]
    public async Task<ActionResult<NominaDto>> VolverAPreparada(int id)
    {
        return await _nominaServicio.VolverAPreparada(id);
    }

    [HttpGet("{id:int}/resumen-rubros")]
    public async Task<ActionResult<List<ResumenRubroDto>>> ObtenerResumenRubros(int id)
    {
        return await _nominaServicio.ObtenerResumenRubros(id);
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
