namespace AutoFlow_Backend.Application.DTOs.Dashboard;

public class LowStockPartDashboardResponse
{
    public Guid PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int MinimumStockLevel { get; set; }
}
