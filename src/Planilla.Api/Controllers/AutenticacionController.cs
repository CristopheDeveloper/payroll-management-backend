using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Interfaces;

namespace Planilla.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutenticacionController : ControllerBase
{
    private readonly IAutenticacionServicio _autenticacionServicio;

    public AutenticacionController(IAutenticacionServicio autenticacionServicio)
    {
        _autenticacionServicio = autenticacionServicio;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginRespuestaDto>> Login(LoginDto dto)
    {
        return await _autenticacionServicio.Login(dto);
    }
}
