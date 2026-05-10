using AutoFlow_Backend.Application.DTOs.Sales;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Mappers;

public static class SaleMapper
{
    public static SaleResponse ToResponse(Sale sale)
    {
        return new SaleResponse
        {
            Id = sale.Id,
            InvoiceNumber = sale.InvoiceNumber,
            CustomerId = sale.CustomerId,
            CustomerName = sale.Customer is not null
                ? sale.Customer.FullName
                : string.Empty,
            StaffId = sale.StaffId,
            SaleDate = sale.SaleDate,
            SubTotal = sale.SubTotal,
            DiscountAmount = sale.DiscountAmount,
            TotalAmount = sale.TotalAmount,
            LoyaltyDiscountApplied = sale.DiscountAmount > 0,
            PaymentMethod = sale.PaymentMethod,
            Status = sale.Status,
            Notes = sale.Notes,
            InvoiceSentAt = sale.InvoiceSentAt,
            InvoiceEmail = sale.InvoiceEmail,
            InvoiceFailedAt = sale.InvoiceFailedAt,
            InvoiceFailureReason = sale.InvoiceFailureReason,
            CreatedAt = sale.CreatedAt,
            Items = sale.SaleItems.Select(si => new SaleItemResponse
            {
                Id = si.Id,
                PartId = si.PartId,
                PartName = si.Part?.PartName ?? string.Empty,
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                SubTotal = si.SubTotal
            }).ToList()
        };
    }

    public static SaleResponse ToSaleResponse(Sale sale)
    {
        return new SaleResponse
        {
            Id = sale.Id,
            CustomerId = sale.CustomerId,
            CustomerName = sale.Customer?.FullName ?? string.Empty,
            StaffId = sale.StaffId,
            SaleDate = sale.SaleDate,
            SubTotal = sale.SubTotal,
            DiscountAmount = sale.DiscountAmount,
            TotalAmount = sale.TotalAmount,
            LoyaltyDiscountApplied = sale.DiscountAmount > 0,
            PaymentMethod = sale.PaymentMethod,
            Status = sale.Status,
            Notes = sale.Notes,
            CreatedAt = sale.CreatedAt,
            Items = sale.SaleItems.Select(item => new SaleItemResponse
            {
                Id = item.Id,
                PartId = item.PartId,
                PartName = item.Part?.PartName ?? string.Empty,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                SubTotal = item.SubTotal
            }).ToList()
        };
    }

    public static SaleInvoiceDto ToInvoiceDto(Sale sale)
    {
        return new SaleInvoiceDto
        {
            InvoiceNumber = sale.InvoiceNumber,
            SaleDate = sale.SaleDate,
            PaymentMethod = sale.PaymentMethod.ToString(),
            SubTotal = sale.SubTotal,
            DiscountAmount = sale.DiscountAmount,
            TotalAmount = sale.TotalAmount,
            CustomerName = sale.Customer?.FullName ?? string.Empty,
            CustomerEmail = sale.Customer?.Email ?? string.Empty,
            CustomerPhone = sale.Customer?.Phone,
            StaffName = sale.Staff?.FullName,
            Items = sale.SaleItems.Select(si => new SaleInvoiceItemDto
            {
                PartName = si.Part?.PartName ?? string.Empty,
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                SubTotal = si.SubTotal
            }).ToList()
        };
    }
}