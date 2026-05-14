using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductivitySystem.Application.Interfaces;

namespace ProductivitySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _service;

    public AlertsController(IAlertService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _service.GetAlerts());
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate()
    {
        await _service.GenerateAlerts();

        return Ok();
    }
}
