using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Constantes;

namespace Planilla.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Rrhh}")]
public class ParametroController : ControllerBase
{
    private readonly IParametroServicio _parametroServicio;

    public ParametroController(IParametroServicio parametroServicio)
    {
        _parametroServicio = parametroServicio;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<List<ParametroDto>>> ObtenerTodos()
    {
        return await _parametroServicio.ObtenerTodos();
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ParametroDto>> ObtenerPorId(int id)
    {
        return await _parametroServicio.ObtenerPorId(id);
    }

    [HttpGet("salario-minimo")]
    public async Task<ActionResult<decimal>> ObtenerSalarioMinimo()
    {
        return await _parametroServicio.ObtenerSalarioMinimo();
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ParametroDto>> Editar(int id, ParametroEditarDto dto)
    {
        return await _parametroServicio.Editar(id, dto);
    }
}
