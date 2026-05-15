using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductivitySystem.Application.DTOs;
using ProductivitySystem.Application.Interfaces;
using ProductivitySystem.Application.Services;

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


    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread()
    {
        return Ok(await _service.GetUnread());
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _service.MarkAsRead(id);

        return NoContent();
    }
}
