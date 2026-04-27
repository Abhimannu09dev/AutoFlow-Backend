namespace AutoFlow_Backend.Application.DTOs.Parts;

public class CreatePartRequest
{
    public string? PartName { get; set; }
    public string? PartNumber { get; set; }
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int StockQuantity { get; set; }
    public int? MinimumStockLevel { get; set; }
    public Guid? VendorId { get; set; }
}
