using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/github")]
public class GitHubController : ControllerBase
{
    private readonly GitHubService _service;

    public GitHubController(GitHubService service)
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
