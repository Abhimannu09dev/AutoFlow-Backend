namespace AutoFlow_Backend.Domain.Entities;

public class PartRequest
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
