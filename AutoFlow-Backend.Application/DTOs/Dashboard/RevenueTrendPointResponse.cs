namespace AutoFlow_Backend.Application.DTOs.Dashboard;

public class RevenueTrendPointResponse
{
    public string Label { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int SalesCount { get; set; }
}
