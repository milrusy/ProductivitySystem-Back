using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductivitySystem.Application.Interfaces;

namespace ProductivitySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service)
    {
        _service = service;
    }

    [HttpGet("metrics/csv")]
    public async Task<IActionResult> ExportMetricsCsv()
    {
        var file = await _service.GenerateMetricsCsv();

        return File(
            file,
            "text/csv",
            "metrics-report.csv"
        );
    }

    [HttpGet("metrics/pdf")]
    public async Task<IActionResult> ExportMetricsPdf()
    {
        var file = await _service.GenerateMetricsPdf();

        return File(
            file,
            "application/pdf",
            "metrics-report.pdf"
        );
    }
}
