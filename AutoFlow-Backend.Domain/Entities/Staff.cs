namespace AutoFlow_Backend.Domain.Entities;

public class Staff
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
