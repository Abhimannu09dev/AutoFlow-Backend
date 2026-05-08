namespace AutoFlow_Backend.Application.DTOs.Vehicles;

public class VehicleResponseDto
{
    /// <summary>
    /// Unique identifier for the vehicle
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Vehicle registration/plate number
    /// </summary>
    public string VehicleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Vehicle manufacturer
    /// </summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// Vehicle model name
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Manufacturing year
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Current odometer reading
    /// </summary>
    public int Mileage { get; set; }

    /// <summary>
    /// Vehicle color
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Vehicle Identification Number
    /// </summary>
    public string? VIN { get; set; }

    /// <summary>
    /// Owner's user ID (ApplicationUser)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Date and time when the vehicle was registered
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the vehicle was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}