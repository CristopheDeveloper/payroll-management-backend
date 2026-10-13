using Microsoft.AspNetCore.Mvc;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Entidades;

namespace Planilla.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolController : ControllerBase
{
    private readonly IRolServicio _rolServicio;

    public RolController(IRolServicio rolServicio)
    {
        _rolServicio = rolServicio;
    }

    [HttpGet]
    public async Task<ActionResult<List<Rol>>> ObtenerTodos()
    {
        return await _rolServicio.ObtenerTodos();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Rol>> ObtenerPorId(int id)
    {
        var rol = await _rolServicio.ObtenerPorId(id);
        if (rol == null)
        {
            return NotFound();
        }

        return rol;
    }

    [HttpPost]
    public async Task<ActionResult<Rol>> Crear(Rol rol)
    {
        var creado = await _rolServicio.Crear(rol);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }
}
