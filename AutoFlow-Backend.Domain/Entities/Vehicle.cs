namespace AutoFlow_Backend.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? Brand { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Customer? Customer { get; set; }
}
