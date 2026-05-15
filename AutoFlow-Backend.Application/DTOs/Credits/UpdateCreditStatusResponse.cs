namespace AutoFlow_Backend.Application.DTOs.Credits;

public class UpdateCreditStatusResponse
{
    public Guid SaleId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
