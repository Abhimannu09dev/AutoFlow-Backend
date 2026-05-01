namespace AutoFlow_Backend.Domain.Entities;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
