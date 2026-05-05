namespace AutoFlow_Backend.Application.DTOs.PurchaseInvoices;

public class PurchaseInvoiceItemRequest
{
    public Guid PartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
}