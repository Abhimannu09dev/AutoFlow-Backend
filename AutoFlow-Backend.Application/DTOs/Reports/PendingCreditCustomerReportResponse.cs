namespace AutoFlow_Backend.Application.DTOs.Reports;

public class PendingCreditCustomerReportResponse
{
    public Guid SaleId { get; set; }
    public Guid CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal CreditAmount { get; set; }
    public int DaysOverdue { get; set; }
}
