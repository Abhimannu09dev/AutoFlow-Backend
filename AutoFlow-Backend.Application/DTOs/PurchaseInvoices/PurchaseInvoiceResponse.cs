using AutoFlow_Backend.Domain.Enums;

namespace AutoFlow_Backend.Application.DTOs.PurchaseInvoices;

public class PurchaseInvoiceResponse
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public Guid CreatedByStaffId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public PurchaseInvoiceStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PurchaseInvoiceItemResponse> Items { get; set; } = new();
}