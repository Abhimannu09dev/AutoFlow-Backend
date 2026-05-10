using AutoFlow_Backend.Application.DTOs.Parts;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Domain.Enums;

namespace AutoFlow_Backend.Application.Mappers;

public static class PartMapper
{
    public static PartResponse ToResponse(this Part part)
    {
        var vendorName = part.Vendor is { IsActive: true } ? part.Vendor.VendorName : null;
        return part.ToResponse(vendorName);
    }

    public static PartResponse ToResponse(this Part part, string? vendorName)
    {
        return new PartResponse
        {
            Id = part.Id,
            PartName = part.PartName,
            PartNumber = part.PartNumber,
            Brand = part.Brand,
            Category = part.Category,
            Description = part.Description,
            UnitPrice = part.UnitPrice,
            SellingPrice = part.SellingPrice,
            StockQuantity = part.StockQuantity,
            MinimumStockLevel = part.MinimumStockLevel,
            StockStatus = GetStockStatus(part.StockQuantity, part.MinimumStockLevel),
            VendorId = part.VendorId,
            VendorName = vendorName,
            IsActive = part.IsActive,
            CreatedAt = part.CreatedAt,
            UpdatedAt = part.UpdatedAt
        };
    }

    private static StockStatus GetStockStatus(int stockQuantity, int minimumStockLevel)
    {
        if (stockQuantity <= 0)
            return StockStatus.OutOfStock;

        if (stockQuantity < minimumStockLevel)
            return StockStatus.LowStock;

        return StockStatus.InStock;
    }
}