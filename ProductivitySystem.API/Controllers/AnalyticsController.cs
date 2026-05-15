using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _service;

    public AnalyticsController(IAnalyticsService service)
    {
        _service = service;
    }

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments()
    {
        return Ok(await _service.GetDepartmentAnalytics());
    }

    [HttpGet("task-distribution")]
    public async Task<IActionResult> GetTaskDistribution()
    {
        return Ok(await _service.GetTaskDistribution());
    }
}
