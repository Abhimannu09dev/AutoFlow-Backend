namespace AutoFlow_Backend.Application.DTOs.Dashboard;

public class PriorityAlertResponse
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
