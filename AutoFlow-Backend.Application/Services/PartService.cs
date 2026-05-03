using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Parts;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Domain.Enums;

namespace AutoFlow_Backend.Application.Services;

public class PartService : IPartService
{
    private const int PartNameMaxLength = 150;
    private const int PartNumberMaxLength = 100;
    private const int BrandMaxLength = 100;
    private const int CategoryMaxLength = 100;
    private const int DescriptionMaxLength = 500;
    private const int DefaultMinimumStockLevel = 10;

    private readonly IPartRepository _partRepository;

    public PartService(IPartRepository partRepository)
    {
        _partRepository = partRepository;
    }

    public async Task<ApiResponse<PartResponse>> CreateAsync(
        CreatePartRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = ValidateCreateOrUpdate(
            request.PartName,
            request.PartNumber,
            request.UnitPrice,
            request.SellingPrice,
            request.StockQuantity,
            request.MinimumStockLevel,
            request.Brand,
            request.Category,
            request.Description,
            request.VendorId);

        if (errors.Count > 0)
        {
            return FailFromValidation<PartResponse>(errors);
        }

        var normalizedPartNumber = request.PartNumber!.Trim().ToLowerInvariant();
        var duplicateExists = await _partRepository.ExistsActiveByPartNumberAsync(normalizedPartNumber, null, cancellationToken);

        if (duplicateExists)
        {
            return Fail<PartResponse>("Duplicate part number is not allowed.");
        }

        string? vendorName = null;
        if (request.VendorId.HasValue)
        {
            vendorName = await _partRepository.GetActiveVendorNameByIdAsync(request.VendorId.Value, cancellationToken);
            if (vendorName is null)
            {
                return Fail<PartResponse>("Vendor not found.");
            }
        }

        var part = new Part
        {
            Id = Guid.NewGuid(),
            PartName = request.PartName!.Trim(),
            PartNumber = request.PartNumber!.Trim(),
            Brand = NormalizeOptional(request.Brand),
            Category = NormalizeOptional(request.Category),
            Description = NormalizeOptional(request.Description),
            UnitPrice = request.UnitPrice,
            SellingPrice = request.SellingPrice,
            StockQuantity = request.StockQuantity,
            MinimumStockLevel = request.MinimumStockLevel ?? DefaultMinimumStockLevel,
            VendorId = request.VendorId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _partRepository.AddAsync(part, cancellationToken);
        await _partRepository.SaveChangesAsync(cancellationToken);

        return Success("Part created successfully.", Map(part, vendorName));
    }

    public async Task<ApiResponse<List<PartResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var parts = await _partRepository.GetActiveAsync(cancellationToken);
        return Success("Parts retrieved successfully.", parts.Select(Map).ToList());
    }

    public async Task<ApiResponse<PartResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var part = await _partRepository.GetActiveByIdAsync(id, cancellationToken);

        if (part is null)
        {
            return Fail<PartResponse>("Part not found.");
        }

        return Success("Part retrieved successfully.", Map(part));
    }

    public async Task<ApiResponse<PartResponse>> UpdateAsync(
        Guid id,
        UpdatePartRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = ValidateCreateOrUpdate(
            request.PartName,
            request.PartNumber,
            request.UnitPrice,
            request.SellingPrice,
            request.StockQuantity,
            request.MinimumStockLevel,
            request.Brand,
            request.Category,
            request.Description,
            request.VendorId);

        if (errors.Count > 0)
        {
            return FailFromValidation<PartResponse>(errors);
        }

        var part = await _partRepository.GetActiveByIdForUpdateAsync(id, cancellationToken);
        if (part is null)
        {
            return Fail<PartResponse>("Part not found.");
        }

        var normalizedPartNumber = request.PartNumber!.Trim().ToLowerInvariant();
        var duplicateExists = await _partRepository.ExistsActiveByPartNumberAsync(normalizedPartNumber, id, cancellationToken);

        if (duplicateExists)
        {
            return Fail<PartResponse>("Duplicate part number is not allowed.");
        }

        string? vendorName = null;
        if (request.VendorId.HasValue)
        {
            vendorName = await _partRepository.GetActiveVendorNameByIdAsync(request.VendorId.Value, cancellationToken);
            if (vendorName is null)
            {
                return Fail<PartResponse>("Vendor not found.");
            }
        }

        part.PartName = request.PartName!.Trim();
        part.PartNumber = request.PartNumber!.Trim();
        part.Brand = NormalizeOptional(request.Brand);
        part.Category = NormalizeOptional(request.Category);
        part.Description = NormalizeOptional(request.Description);
        part.UnitPrice = request.UnitPrice;
        part.SellingPrice = request.SellingPrice;
        part.StockQuantity = request.StockQuantity;
        part.MinimumStockLevel = request.MinimumStockLevel ?? DefaultMinimumStockLevel;
        part.VendorId = request.VendorId;
        part.UpdatedAt = DateTime.UtcNow;

        _partRepository.Update(part);
        await _partRepository.SaveChangesAsync(cancellationToken);

        return Success("Part updated successfully.", Map(part, vendorName));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var part = await _partRepository.GetActiveByIdForUpdateAsync(id, cancellationToken);

        if (part is null)
        {
            return Fail<bool>("Part not found.");
        }

        part.IsActive = false;
        part.UpdatedAt = DateTime.UtcNow;

        _partRepository.Update(part);
        await _partRepository.SaveChangesAsync(cancellationToken);

        return Success("Part deleted successfully.", true);
    }

    public async Task<ApiResponse<List<PartResponse>>> SearchAsync(
        string? query,
        CancellationToken cancellationToken = default)
    {
        var parts = await _partRepository.SearchActiveAsync(query, cancellationToken);
        return Success("Parts retrieved successfully.", parts.Select(Map).ToList());
    }

    public async Task<ApiResponse<List<PartResponse>>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        var parts = await _partRepository.GetLowStockActiveAsync(cancellationToken);
        return Success("Low-stock parts retrieved successfully.", parts.Select(Map).ToList());
    }

    private static PartResponse Map(Part part)
    {
        var vendorName = part.Vendor is { IsActive: true } ? part.Vendor.VendorName : null;
        return Map(part, vendorName);
    }

    private static PartResponse Map(Part part, string? vendorName)
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

    private static List<string> ValidateCreateOrUpdate(
        string? partName,
        string? partNumber,
        decimal unitPrice,
        decimal sellingPrice,
        int stockQuantity,
        int? minimumStockLevel,
        string? brand,
        string? category,
        string? description,
        Guid? vendorId)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(partName))
        {
            errors.Add("Part name is required.");
        }
        else if (partName.Trim().Length > PartNameMaxLength)
        {
            errors.Add($"Part name must be at most {PartNameMaxLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(partNumber))
        {
            errors.Add("Part number is required.");
        }
        else if (partNumber.Trim().Length > PartNumberMaxLength)
        {
            errors.Add($"Part number must be at most {PartNumberMaxLength} characters.");
        }

        if (unitPrice < 0)
        {
            errors.Add("Unit price must be greater than or equal to 0.");
        }

        if (sellingPrice < 0)
        {
            errors.Add("Selling price must be greater than or equal to 0.");
        }

        if (stockQuantity < 0)
        {
            errors.Add("Stock quantity must be greater than or equal to 0.");
        }

        var minimumStock = minimumStockLevel ?? DefaultMinimumStockLevel;
        if (minimumStock < 0)
        {
            errors.Add("Minimum stock level must be greater than or equal to 0.");
        }

        if (!string.IsNullOrWhiteSpace(brand) && brand.Trim().Length > BrandMaxLength)
        {
            errors.Add($"Brand must be at most {BrandMaxLength} characters.");
        }

        if (!string.IsNullOrWhiteSpace(category) && category.Trim().Length > CategoryMaxLength)
        {
            errors.Add($"Category must be at most {CategoryMaxLength} characters.");
        }

        if (!string.IsNullOrWhiteSpace(description) && description.Trim().Length > DescriptionMaxLength)
        {
            errors.Add($"Description must be at most {DescriptionMaxLength} characters.");
        }

        if (vendorId.HasValue && vendorId.Value == Guid.Empty)
        {
            errors.Add("VendorId is invalid.");
        }

        return errors;
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static StockStatus GetStockStatus(int stockQuantity, int minimumStockLevel)
    {
        if (stockQuantity <= 0)
        {
            return StockStatus.OutOfStock;
        }

        if (stockQuantity < minimumStockLevel)
        {
            return StockStatus.LowStock;
        }

        return StockStatus.InStock;
    }

    private static ApiResponse<T> Success<T>(string message, T data)
    {
        return new ApiResponse<T>
        {
            Status = true,
            Message = message,
            Data = data
        };
    }

    private static ApiResponse<T> Fail<T>(string message)
    {
        return new ApiResponse<T>
        {
            Status = false,
            Message = message,
            Data = default
        };
    }

    private static ApiResponse<T> FailFromValidation<T>(List<string> errors)
    {
        var message = errors.Count > 0 ? string.Join(" ", errors) : "Validation failed.";
        return Fail<T>(message);
    }
}
