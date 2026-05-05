namespace AutoFlow_Backend.Application.DTOs.PurchaseInvoices;

public class CreatePurchaseInvoiceRequest
{
    public Guid VendorId { get; set; }
    public string? Notes { get; set; }
    public List<PurchaseInvoiceItemRequest> Items { get; set; } = new();
}