namespace AutoFlow_Backend.Application.DTOs.PartRequests;

public class PartRequestResponse
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
