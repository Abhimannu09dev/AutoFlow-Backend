namespace AutoFlow_Backend.Application.DTOs.Appointments;

public class CreateAppointmentRequest
{
    public Guid CustomerId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public string? Status { get; set; }
}
