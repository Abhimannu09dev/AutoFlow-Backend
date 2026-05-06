namespace AutoFlow_Backend.Application.DTOs.Reports;

public class FinancialReportResponse
{
    public string Period { get; set; } = string.Empty;
    public int TotalSales { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal NetRevenue { get; set; }
}