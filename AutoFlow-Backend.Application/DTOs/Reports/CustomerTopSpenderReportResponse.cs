namespace AutoFlow_Backend.Application.DTOs.Reports;

public class CustomerTopSpenderReportResponse
{
    public Guid CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public int PurchaseCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime LastPurchaseDate { get; set; }
}
