namespace AutoFlow_Backend.Application.DTOs.Parts;

public class PartResponse
{
    public Guid Id { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int StockQuantity { get; set; }
    public int MinimumStockLevel { get; set; }
    public Guid? VendorId { get; set; }
    public string? VendorName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
