namespace AutoFlow_Backend.Application.DTOs.Customers;

public class VehicleResponseDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? Brand { get; set; }
    public DateTime CreatedAt { get; set; }
}
