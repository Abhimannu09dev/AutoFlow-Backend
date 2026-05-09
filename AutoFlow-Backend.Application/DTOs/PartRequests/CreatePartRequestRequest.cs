using System.ComponentModel.DataAnnotations;

namespace AutoFlow_Backend.Application.DTOs.PartRequests;

public class CreatePartRequestRequest
{
    public Guid? CustomerId { get; set; }

    [Required]
    [MaxLength(200)]
    public string PartName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    public string? Status { get; set; }
}
