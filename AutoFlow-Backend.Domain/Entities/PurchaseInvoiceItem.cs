namespace AutoFlow_Backend.Domain.Entities;

public class PurchaseInvoiceItem
{
    public Guid Id { get; set; }
    public Guid PurchaseInvoiceId { get; set; }
    public Guid PartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }    
    public decimal SubTotal { get; set; }    

    public PurchaseInvoice? PurchaseInvoice { get; set; }
    public Part? Part { get; set; }
}