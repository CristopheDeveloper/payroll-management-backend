using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Interfaces;
using Planilla.Dominio.Constantes;

namespace Planilla.Api.Controllers;

[ApiController]
[Route("api/[controller]/{idNomina:int}")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Rrhh}")]
public class ReporteController : ControllerBase
{
    private readonly IReporteServicio _reporteServicio;
    private readonly IGeneradorDocumentos _generadorDocumentos;

    public ReporteController(IReporteServicio reporteServicio, IGeneradorDocumentos generadorDocumentos)
    {
        _reporteServicio = reporteServicio;
        _generadorDocumentos = generadorDocumentos;
    }

    [HttpGet("planilla")]
    public async Task<ActionResult<ReportePlanillaDto>> ObtenerPlanilla(int idNomina)
    {
        return await _reporteServicio.ObtenerPlanilla(idNomina);
    }

    [HttpGet("planilla/pdf")]
    public async Task<IActionResult> DescargarPlanillaPdf(int idNomina)
    {
        var reporte = await _reporteServicio.ObtenerPlanilla(idNomina);
        return File(_generadorDocumentos.PlanillaPdf(reporte), "application/pdf",
            $"planilla-{reporte.Encabezado.Anio}-{reporte.Encabezado.Mes:00}.pdf");
    }

    [HttpGet("planilla/excel")]
    public async Task<IActionResult> DescargarPlanillaExcel(int idNomina)
    {
        var reporte = await _reporteServicio.ObtenerPlanilla(idNomina);
        return File(_generadorDocumentos.PlanillaExcel(reporte),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"planilla-{reporte.Encabezado.Anio}-{reporte.Encabezado.Mes:00}.xlsx");
    }

    [HttpGet("boletas")]
    public async Task<ActionResult<ReporteBoletasDto>> ObtenerBoletas(int idNomina, int? idEmpleado = null)
    {
        return await _reporteServicio.ObtenerBoletas(idNomina, idEmpleado);
    }

    [HttpGet("boletas/pdf")]
    public async Task<IActionResult> DescargarBoletasPdf(int idNomina, int? idEmpleado = null)
    {
        var reporte = await _reporteServicio.ObtenerBoletas(idNomina, idEmpleado);
        var sufijo = idEmpleado == null ? "" : $"-{reporte.Boletas[0].CodigoEmpleado}";
        return File(_generadorDocumentos.BoletasPdf(reporte), "application/pdf",
            $"boletas-{reporte.Encabezado.Anio}-{reporte.Encabezado.Mes:00}{sufijo}.pdf");
    }
}
