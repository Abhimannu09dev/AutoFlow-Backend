namespace AutoFlow_Backend.Domain.Entities;

public class Part
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
    public int MinimumStockLevel { get; set; } = 10;
    public Guid? VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
