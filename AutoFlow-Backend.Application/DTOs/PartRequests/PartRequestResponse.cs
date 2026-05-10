using AutoFlow_Backend.Domain.Enums;

namespace AutoFlow_Backend.Application.DTOs.PartRequests;

public class PartRequestResponse
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public PartRequestStatus Status { get; set; } = PartRequestStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
