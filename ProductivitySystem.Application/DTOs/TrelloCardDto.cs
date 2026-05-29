namespace ProductivitySystem.Application.DTOs; 

public class TrelloCardDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime? Due { get; set; }

    public string ListName { get; set; }

    public List<string> MemberIds { get; set; } = new();
}
