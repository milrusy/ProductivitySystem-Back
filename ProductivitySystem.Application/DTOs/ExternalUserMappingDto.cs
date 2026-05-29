namespace ProductivitySystem.Application.DTOs; 

public class ExternalUserMappingDto
{
    public int UserId { get; set; }
    public string? GitHubLogin { get; set; }
    public string? TrelloMemberId { get; set; }
}
