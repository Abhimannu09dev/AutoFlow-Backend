namespace AutoFlow_Backend.Application.DTOs.Dashboard;

public class ActivityStreamItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ActorName { get; set; } = string.Empty;
}
