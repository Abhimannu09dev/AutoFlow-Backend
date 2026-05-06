namespace AutoFlow_Backend.Application.DTOs.Dashboard;

public class DashboardResponse
{
    public int TotalSalesCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalCustomersCount { get; set; }
    public int TotalStaffCount { get; set; }
    public List<LowStockPartDashboardResponse> LowStockParts { get; set; } = new();
}
