namespace AutoFlow_Backend.Application.DTOs.PartRequests;

public class CreatePartRequestRequest
{
    public Guid CustomerId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Status { get; set; }
}
