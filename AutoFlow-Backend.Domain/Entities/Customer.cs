namespace AutoFlow_Backend.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? ApplicationUserId { get; set; }
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}