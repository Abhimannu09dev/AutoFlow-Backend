namespace AutoFlow_Backend.Application.DTOs.Dashboard;

public class DashboardResponse
{
    public int TotalSalesCount { get; set; }
    public int CompletedSalesCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal YearlyRevenue { get; set; }
    public int TotalCustomersCount { get; set; }
    public int TotalStaffCount { get; set; }
    public int TotalVendorsCount { get; set; }
    public int TotalPartsCount { get; set; }
    public int TotalPurchaseInvoicesCount { get; set; }
    public int PendingInvoicesCount { get; set; }
    public int TotalAppointmentsCount { get; set; }
    public int PendingAppointmentsCount { get; set; }
    public int PendingPartRequestsCount { get; set; }
    public int TotalReviewsCount { get; set; }
    public decimal AverageReviewRating { get; set; }
    public int LowStockPartsCount { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public List<LowStockPartDashboardResponse> LowStockParts { get; set; } = new();
}
