using System.ComponentModel.DataAnnotations;

namespace AutoFlow_Backend.Application.DTOs.Vehicles;

public class VehicleCreateDto
{
    [Required]
    [MaxLength(20)]
    public string VehicleNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Model { get; set; } = string.Empty;

    [Range(1886, 3000)]
    public int Year { get; set; }

    [MaxLength(30)]
    public string? Color { get; set; }

    [MaxLength(50)]
    public string? VIN { get; set; }
}