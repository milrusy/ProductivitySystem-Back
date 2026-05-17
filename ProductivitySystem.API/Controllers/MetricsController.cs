using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductivitySystem.Application.Interfaces;

namespace ProductivitySystem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly IMetricsService _service;

    public MetricsController(IMetricsService service)
    {
        _service = service;
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserMetrics(
        int userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var result = await _service.GetUserMetrics(userId, from, to);
        return Ok(result);
    }

    [HttpGet("trends")]
    public async Task<IActionResult> GetTrends(
    [FromQuery] int? userId,
    [FromQuery] int? departmentId,
    DateTime? startDate,
    DateTime? endDate)
    {
        var result = await _service.GetTrends(userId, departmentId, startDate, endDate);
        return Ok(result);
    }

    [HttpGet("departments")]
    public async Task<IActionResult>
        GetDepartmentAnalytics()
    {
        return Ok(
            await _service
                .GetDepartmentAnalytics(null));
    }

    [HttpGet("departments/{departmentId}")]
    public async Task<IActionResult>
        GetDepartmentAnalytics(int? departmentId)
    {
        return Ok(
            await _service
                .GetDepartmentAnalytics(departmentId));
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployeesPerformance()
    {
        return Ok(await _service.GetEmployeePerformance());
    }
}
