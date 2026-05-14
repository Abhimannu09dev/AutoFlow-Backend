namespace AutoFlow_Backend.Application.DTOs.Dashboard;

public class FastMovingInventoryResponse
{
    public Guid PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public int SoldQuantity { get; set; }
    public int CurrentStock { get; set; }
    public decimal Revenue { get; set; }
}
