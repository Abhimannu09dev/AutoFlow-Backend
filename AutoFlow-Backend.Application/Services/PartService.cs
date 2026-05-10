using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Parts;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Application.Mappers;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Domain.Enums;
using FluentValidation;

namespace AutoFlow_Backend.Application.Services;

public class PartService : IPartService
{
    private const int DefaultMinimumStockLevel = 10;

    private readonly IPartRepository _partRepository;
    private readonly IValidator<CreatePartRequest> _createValidator;
    private readonly IValidator<UpdatePartRequest> _updateValidator;

    public PartService(
        IPartRepository partRepository,
        IValidator<CreatePartRequest> createValidator,
        IValidator<UpdatePartRequest> updateValidator)
    {
        _partRepository = partRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ApiResponse<PartResponse>> CreateAsync(
        CreatePartRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return ApiResponseFactory.FailFromValidation<PartResponse>(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var normalizedPartNumber = request.PartNumber!.Trim().ToLowerInvariant();
        var duplicateExists = await _partRepository.ExistsActiveByPartNumberAsync(normalizedPartNumber, null, cancellationToken);

        if (duplicateExists)
        {
            return ApiResponseFactory.Fail<PartResponse>("Duplicate part number is not allowed.");
        }

        string? vendorName = null;
        if (request.VendorId.HasValue)
        {
            vendorName = await _partRepository.GetActiveVendorNameByIdAsync(request.VendorId.Value, cancellationToken);
            if (vendorName is null)
            {
                return ApiResponseFactory.Fail<PartResponse>("Vendor not found.");
            }
        }

        var part = new Part
        {
            Id = Guid.NewGuid(),
            PartName = request.PartName!.Trim(),
            PartNumber = request.PartNumber!.Trim(),
            Brand = StringNormalizer.NormalizeOptional(request.Brand),
            Category = StringNormalizer.NormalizeOptional(request.Category),
            Description = StringNormalizer.NormalizeOptional(request.Description),
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

        return ApiResponseFactory.Ok("Part created successfully.", part.ToResponse(vendorName));
    }

    public async Task<ApiResponse<List<PartResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var parts = await _partRepository.GetActiveAsync(cancellationToken);
        return ApiResponseFactory.Ok("Parts retrieved successfully.", parts.Select(p => p.ToResponse()).ToList());
    }

    public async Task<ApiResponse<PartResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var part = await _partRepository.GetActiveByIdAsync(id, cancellationToken);

        if (part is null)
        {
            return ApiResponseFactory.Fail<PartResponse>("Part not found.", ErrorType.NotFound);
        }

        return ApiResponseFactory.Ok("Part retrieved successfully.", part.ToResponse());
    }

    public async Task<ApiResponse<PartResponse>> UpdateAsync(
        Guid id,
        UpdatePartRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return ApiResponseFactory.FailFromValidation<PartResponse>(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var part = await _partRepository.GetActiveByIdForUpdateAsync(id, cancellationToken);
        if (part is null)
        {
            return ApiResponseFactory.Fail<PartResponse>("Part not found.", ErrorType.NotFound);
        }

        var normalizedPartNumber = request.PartNumber!.Trim().ToLowerInvariant();
        var duplicateExists = await _partRepository.ExistsActiveByPartNumberAsync(normalizedPartNumber, id, cancellationToken);

        if (duplicateExists)
        {
            return ApiResponseFactory.Fail<PartResponse>("Duplicate part number is not allowed.");
        }

        string? vendorName = null;
        if (request.VendorId.HasValue)
        {
            vendorName = await _partRepository.GetActiveVendorNameByIdAsync(request.VendorId.Value, cancellationToken);
            if (vendorName is null)
            {
                return ApiResponseFactory.Fail<PartResponse>("Vendor not found.");
            }
        }

        part.PartName = request.PartName!.Trim();
        part.PartNumber = request.PartNumber!.Trim();
        part.Brand = StringNormalizer.NormalizeOptional(request.Brand);
        part.Category = StringNormalizer.NormalizeOptional(request.Category);
        part.Description = StringNormalizer.NormalizeOptional(request.Description);
        part.UnitPrice = request.UnitPrice;
        part.SellingPrice = request.SellingPrice;
        part.StockQuantity = request.StockQuantity;
        part.MinimumStockLevel = request.MinimumStockLevel ?? DefaultMinimumStockLevel;
        part.VendorId = request.VendorId;
        part.UpdatedAt = DateTime.UtcNow;

        _partRepository.Update(part);
        await _partRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Part updated successfully.", part.ToResponse(vendorName));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var part = await _partRepository.GetActiveByIdForUpdateAsync(id, cancellationToken);

        if (part is null)
        {
            return ApiResponseFactory.Fail<bool>("Part not found.", ErrorType.NotFound);
        }

        part.IsActive = false;
        part.UpdatedAt = DateTime.UtcNow;

        _partRepository.Update(part);
        await _partRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Part deleted successfully.", true);
    }

    public async Task<ApiResponse<List<PartResponse>>> SearchAsync(
        string? query,
        CancellationToken cancellationToken = default)
    {
        var parts = await _partRepository.SearchActiveAsync(query, cancellationToken);
        return ApiResponseFactory.Ok("Parts retrieved successfully.", parts.Select(p => p.ToResponse()).ToList());
    }

    public async Task<ApiResponse<List<PartResponse>>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        var parts = await _partRepository.GetLowStockActiveAsync(cancellationToken);
        return ApiResponseFactory.Ok("Low-stock parts retrieved successfully.", parts.Select(p => p.ToResponse()).ToList());
    }
}
