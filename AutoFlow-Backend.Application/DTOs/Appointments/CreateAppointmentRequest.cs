using System.ComponentModel.DataAnnotations;

namespace AutoFlow_Backend.Application.DTOs.Appointments;

public class CreateAppointmentRequest
{
    public Guid? CustomerId { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public TimeOnly Time { get; set; }

    public string? Status { get; set; }

    public Guid? VehicleId { get; set; }
}
