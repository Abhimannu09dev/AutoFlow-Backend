using System.ComponentModel.DataAnnotations;

namespace AutoFlow_Backend.Application.DTOs.Vehicles;

public class VehicleUpdateDto
{
    /// <summary>
    /// Updated vehicle registration/plate number
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string VehicleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Updated vehicle manufacturer
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// Updated vehicle model name
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Updated manufacturing year
    /// </summary>
    [Required]
    [Range(1886, 3000)]
    public int Year { get; set; }

    /// <summary>
    /// Updated odometer reading (Staff/Admin only can update)
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? Mileage { get; set; }

    /// <summary>
    /// Updated vehicle color (optional)
    /// </summary>
    [MaxLength(30)]
    public string? Color { get; set; }

    /// <summary>
    /// Updated Vehicle Identification Number (optional)
    /// </summary>
    [MaxLength(50)]
    public string? VIN { get; set; }
}