namespace AutoFlow_Backend.Application.DTOs.Credits;

public class SendCreditReminderResponse
{
    public Guid SaleId { get; set; }
    public DateTime SentAt { get; set; }
    public string Channel { get; set; } = "Email";
}
