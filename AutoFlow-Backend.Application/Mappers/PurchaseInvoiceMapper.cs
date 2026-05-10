using AutoFlow_Backend.Application.DTOs.PurchaseInvoices;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Mappers;

public static class PurchaseInvoiceMapper
{
    public static PurchaseInvoiceResponse ToResponse(PurchaseInvoice invoice)
    {
        return ToResponse(invoice, invoice.Vendor?.VendorName ?? string.Empty);
    }

    public static PurchaseInvoiceResponse ToResponse(PurchaseInvoice invoice, string vendorName)
    {
        return new PurchaseInvoiceResponse
        {
            Id = invoice.Id,
            VendorId = invoice.VendorId,
            VendorName = vendorName,
            CreatedByStaffId = invoice.CreatedByStaffId,
            InvoiceDate = invoice.InvoiceDate,
            TotalAmount = invoice.TotalAmount,
            Status = invoice.Status,
            Notes = invoice.Notes,
            CreatedAt = invoice.CreatedAt,
            Items = invoice.Items.Select(i => new PurchaseInvoiceItemResponse
            {
                Id = i.Id,
                PartId = i.PartId,
                PartName = i.Part?.PartName ?? string.Empty,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                SubTotal = i.SubTotal
            }).ToList()
        };
    }
}