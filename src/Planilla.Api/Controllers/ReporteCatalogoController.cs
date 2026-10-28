using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Excepciones;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Constantes;

namespace Planilla.Api.Controllers;

[ApiController]
[Route("api/Reporte")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Rrhh}")]
public class ReporteCatalogoController : ControllerBase
{
    private readonly IReporteCatalogoServicio _reporteCatalogoServicio;
    private readonly IGeneradorDocumentos _generadorDocumentos;

    public ReporteCatalogoController(IReporteCatalogoServicio reporteCatalogoServicio,
        IGeneradorDocumentos generadorDocumentos)
    {
        _reporteCatalogoServicio = reporteCatalogoServicio;
        _generadorDocumentos = generadorDocumentos;
    }

    [HttpGet("planilla/{idNomina:int}/general")]
    public async Task<IActionResult> PlanillaGeneral(int idNomina, string formato)
    {
        return Responder(await _reporteCatalogoServicio.PlanillaGeneral(idNomina), formato);
    }

    [HttpGet("planilla/{idNomina:int}/boletas")]
    public async Task<IActionResult> Boletas(int idNomina, string formato)
    {
        return Responder(await _reporteCatalogoServicio.Boletas(idNomina), formato);
    }

    [HttpGet("planilla/{idNomina:int}/forma-pago")]
    public async Task<IActionResult> FormaPago(int idNomina, string tipoPago, string formato)
    {
        return Responder(await _reporteCatalogoServicio.FormaPago(idNomina, tipoPago), formato);
    }

    [HttpGet("planilla/{idNomina:int}/rubros")]
    public async Task<IActionResult> Rubros(int idNomina, int? idRubro, string formato)
    {
        return Responder(await _reporteCatalogoServicio.Rubros(idNomina, idRubro), formato);
    }

    [HttpGet("planilla/{idNomina:int}/igss")]
    public async Task<IActionResult> Igss(int idNomina, string formato)
    {
        return Responder(await _reporteCatalogoServicio.Igss(idNomina), formato);
    }

    [HttpGet("planilla/{idNomina:int}/poliza")]
    public async Task<IActionResult> Poliza(int idNomina, string formato)
    {
        return Responder(await _reporteCatalogoServicio.Poliza(idNomina), formato);
    }

    [HttpGet("empleados")]
    public async Task<IActionResult> Empleados(string estado, int? idDepartamento, int? idPuesto, string formato)
    {
        return Responder(await _reporteCatalogoServicio.Empleados(estado, idDepartamento, idPuesto), formato);
    }

    [HttpGet("altas-bajas")]
    public async Task<IActionResult> AltasBajas([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, string formato)
    {
        if (desde == null || hasta == null)
        {
            throw new ReglaNegocioException("Las fechas inicial y final son obligatorias.");
        }
        return Responder(await _reporteCatalogoServicio.AltasBajas(desde.Value, hasta.Value), formato);
    }

    [HttpGet("historial/{idEmpleado:int}")]
    public async Task<IActionResult> Historial(int idEmpleado, [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta,
        string formato)
    {
        return Responder(await _reporteCatalogoServicio.Historial(idEmpleado, desde, hasta), formato);
    }

    [HttpGet("asistencia")]
    public async Task<IActionResult> Asistencia(int anio, int mes, int? idDepartamento, string formato)
    {
        return Responder(await _reporteCatalogoServicio.Asistencia(anio, mes, idDepartamento), formato);
    }

    private IActionResult Responder(ReporteTablaDto reporte, string formato)
    {
        return formato?.ToLower() switch
        {
            "pdf" => File(_generadorDocumentos.TablaPdf(reporte), "application/pdf", $"{reporte.NombreArchivo}.pdf"),
            "excel" => File(_generadorDocumentos.TablaExcel(reporte),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reporte.NombreArchivo}.xlsx"),
            null or "" => Ok(reporte),
            _ => throw new ReglaNegocioException("El formato debe ser pdf o excel.")
        };
    }
}
