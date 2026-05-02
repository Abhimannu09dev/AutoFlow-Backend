namespace AutoFlow_Backend.Application.DTOs.Appointments;

public class AppointmentResponse
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
