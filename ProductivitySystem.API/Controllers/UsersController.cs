using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductivitySystem.Application.DTOs;
using ProductivitySystem.Application.Interfaces;
using System.Security.Claims;

namespace ProductivitySystem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserManagementService _service;
    private readonly IUserAnalyticsService _analyticsService;

    public UsersController(
    IUserManagementService service,
    IUserAnalyticsService analyticsService)
    {
        _service = service;
        _analyticsService = analyticsService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _service.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _service.GetByIdAsync(id);
        if (user == null) return NotFound();

        return Ok(user);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateUser(
        CreateUserDto dto)
    {
        try
        {
            var tempPassword = await _service.CreateUser(dto);

            return Ok(new
            {
                temporaryPassword = tempPassword
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}/analytics")]
    public async Task<IActionResult> GetAnalytics(int id)
    {
        var result = await _analyticsService
            .GetEmployeeDetails(id);

        return Ok(result);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordDto dto)
    {
        var currentUserId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );

        await _service.ChangePassword(
            currentUserId,
            dto);

        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordDto dto)
    {
        await _service.ResetPassword(dto);

        return Ok();
    }
}
