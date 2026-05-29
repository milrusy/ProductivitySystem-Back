using Microsoft.AspNetCore.Mvc;
using ProductivitySystem.Application.Interfaces;
using ProductivitySystem.Application.Mappers;

namespace ProductivitySystem.API.Controllers;

[ApiController]
[Route("api/github")]
public class GitHubController : ControllerBase
{
    private readonly IGitHubService _service;

    public GitHubController(IGitHubService service)
    {
        _service = service;
    }

    [HttpGet("tasks")]
    public async Task<IActionResult> GetTasks()
    {
        var issues = await _service.GetIssuesAsync();
        var mapped = TaskMapper.Map(issues);

        return Ok(mapped);
    }
}
