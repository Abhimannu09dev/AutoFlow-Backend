namespace AutoFlow_Backend.Application.DTOs.PurchaseInvoices;

public class PurchaseInvoiceItemResponse
{
    public Guid Id { get; set; }
    public Guid PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal SubTotal { get; set; }
}