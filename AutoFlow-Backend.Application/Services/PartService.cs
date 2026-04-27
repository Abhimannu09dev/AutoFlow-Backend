using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Parts;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Application.Services;

public class PartService : IPartService
{
    private const int PartNameMaxLength = 150;
    private const int PartNumberMaxLength = 100;
    private const int BrandMaxLength = 100;
    private const int CategoryMaxLength = 100;
    private const int DescriptionMaxLength = 500;
    private const int DefaultMinimumStockLevel = 10;

    private readonly IAppDbContext _dbContext;

    public PartService(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
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
        var duplicateExists = await _dbContext.Parts
            .AsNoTracking()
            .AnyAsync(p => p.IsActive && p.PartNumber.ToLower() == normalizedPartNumber, cancellationToken);

        if (duplicateExists)
        {
            return Fail<PartResponse>("Duplicate part number is not allowed.");
        }

        string? vendorName = null;
        if (request.VendorId.HasValue)
        {
            var vendor = await _dbContext.Vendors
                .AsNoTracking()
                .Where(v => v.IsActive && v.Id == request.VendorId.Value)
                .Select(v => new { v.Id, v.VendorName })
                .FirstOrDefaultAsync(cancellationToken);

            if (vendor is null)
            {
                return Fail<PartResponse>("Vendor not found.");
            }

            vendorName = vendor.VendorName;
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

        await _dbContext.Parts.AddAsync(part, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Success("Part created successfully.", Map(part, vendorName));
    }

    public async Task<ApiResponse<List<PartResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var parts = await _dbContext.Parts
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Where(p => p.IsActive)
            .OrderBy(p => p.PartName)
            .Select(p => Map(p, p.Vendor != null && p.Vendor.IsActive ? p.Vendor.VendorName : null))
            .ToListAsync(cancellationToken);

        return Success("Parts retrieved successfully.", parts);
    }

    public async Task<ApiResponse<PartResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var part = await _dbContext.Parts
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Where(p => p.IsActive && p.Id == id)
            .Select(p => Map(p, p.Vendor != null && p.Vendor.IsActive ? p.Vendor.VendorName : null))
            .FirstOrDefaultAsync(cancellationToken);

        if (part is null)
        {
            return Fail<PartResponse>("Part not found.");
        }

        return Success("Part retrieved successfully.", part);
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

        var part = await _dbContext.Parts
            .FirstOrDefaultAsync(p => p.IsActive && p.Id == id, cancellationToken);

        if (part is null)
        {
            return Fail<PartResponse>("Part not found.");
        }

        var normalizedPartNumber = request.PartNumber!.Trim().ToLowerInvariant();
        var duplicateExists = await _dbContext.Parts
            .AsNoTracking()
            .AnyAsync(p => p.IsActive && p.Id != id && p.PartNumber.ToLower() == normalizedPartNumber, cancellationToken);

        if (duplicateExists)
        {
            return Fail<PartResponse>("Duplicate part number is not allowed.");
        }

        string? vendorName = null;
        if (request.VendorId.HasValue)
        {
            var vendor = await _dbContext.Vendors
                .AsNoTracking()
                .Where(v => v.IsActive && v.Id == request.VendorId.Value)
                .Select(v => new { v.Id, v.VendorName })
                .FirstOrDefaultAsync(cancellationToken);

            if (vendor is null)
            {
                return Fail<PartResponse>("Vendor not found.");
            }

            vendorName = vendor.VendorName;
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

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Success("Part updated successfully.", Map(part, vendorName));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var part = await _dbContext.Parts
            .FirstOrDefaultAsync(p => p.IsActive && p.Id == id, cancellationToken);

        if (part is null)
        {
            return Fail<bool>("Part not found.");
        }

        part.IsActive = false;
        part.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Success("Part deleted successfully.", true);
    }

    public async Task<ApiResponse<List<PartResponse>>> SearchAsync(
        string? query,
        CancellationToken cancellationToken = default)
    {
        var normalizedQuery = query?.Trim();
        var partQuery = _dbContext.Parts
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            var lowered = normalizedQuery.ToLowerInvariant();
            partQuery = partQuery.Where(p =>
                p.PartName.ToLower().Contains(lowered) ||
                p.PartNumber.ToLower().Contains(lowered) ||
                (p.Brand != null && p.Brand.ToLower().Contains(lowered)) ||
                (p.Category != null && p.Category.ToLower().Contains(lowered)));
        }

        var results = await partQuery
            .OrderBy(p => p.PartName)
            .Select(p => Map(p, p.Vendor != null && p.Vendor.IsActive ? p.Vendor.VendorName : null))
            .ToListAsync(cancellationToken);

        return Success("Parts retrieved successfully.", results);
    }

    public async Task<ApiResponse<List<PartResponse>>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        var parts = await _dbContext.Parts
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Where(p => p.IsActive && p.StockQuantity < p.MinimumStockLevel)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.PartName)
            .Select(p => Map(p, p.Vendor != null && p.Vendor.IsActive ? p.Vendor.VendorName : null))
            .ToListAsync(cancellationToken);

        return Success("Low-stock parts retrieved successfully.", parts);
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
