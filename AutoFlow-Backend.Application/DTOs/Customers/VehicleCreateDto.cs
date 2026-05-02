using System.ComponentModel.DataAnnotations;

namespace AutoFlow_Backend.Application.DTOs.Customers;

public class VehicleCreateDto
{
    [Required]
    [MaxLength(20)]
    public string VehicleNumber { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Model { get; set; }

    [MaxLength(50)]
    public string? Brand { get; set; }
}
