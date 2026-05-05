using AutoFlow_Backend.Domain.Enums;

namespace AutoFlow_Backend.Domain.Entities;

public class PurchaseInvoice
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public Guid CreatedByStaffId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public PurchaseInvoiceStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Vendor? Vendor { get; set; }
    public ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
}