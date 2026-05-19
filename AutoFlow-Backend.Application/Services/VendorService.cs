using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Vendors;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Application.Mappers;
using AutoFlow_Backend.Domain.Entities;
using FluentValidation;

namespace AutoFlow_Backend.Application.Services;

public class VendorService : IVendorService
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IValidator<CreateVendorRequest> _createValidator;
    private readonly IValidator<UpdateVendorRequest> _updateValidator;

    public VendorService(
        IVendorRepository vendorRepository,
        IValidator<CreateVendorRequest> createValidator,
        IValidator<UpdateVendorRequest> updateValidator)
    {
        _vendorRepository = vendorRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ApiResponse<VendorResponse>> CreateAsync(
        CreateVendorRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return ApiResponseFactory.FailFromValidation<VendorResponse>(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var normalizedName = request.VendorName.Trim().ToLowerInvariant();
        var duplicateExists = await _vendorRepository.ExistsActiveByNameAsync(normalizedName, null, cancellationToken);

        if (duplicateExists)
        {
            return ApiResponseFactory.Fail<VendorResponse>("Duplicate vendor name is not allowed.");
        }

        var normalizedEmail = StringNormalizer.NormalizeOptional(request.Email);
        if (!string.IsNullOrWhiteSpace(normalizedEmail))
        {
            var duplicateEmail = await _vendorRepository.ExistsByEmailAsync(
                normalizedEmail.ToLowerInvariant(),
                null,
                cancellationToken);
            if (duplicateEmail)
            {
                return ApiResponseFactory.Fail<VendorResponse>("Vendor email already exists.");
            }
        }

        var vendor = new Vendor
        {
            Id = Guid.NewGuid(),
            VendorName = request.VendorName.Trim(),
            ContactPerson = StringNormalizer.NormalizeOptional(request.ContactPerson),
            Phone = request.Phone.Trim(),
            Email = normalizedEmail,
            Address = StringNormalizer.NormalizeOptional(request.Address),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _vendorRepository.AddAsync(vendor, cancellationToken);
        await _vendorRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Vendor created successfully.", vendor.ToResponse());
    }

    public async Task<ApiResponse<PagedResponse<VendorResponse>>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var paged = await _vendorRepository.GetPagedAsync(request, cancellationToken);
        return ApiResponseFactory.Ok("Vendors retrieved successfully.", paged.Map(v => v.ToResponse()));
    }

    public async Task<ApiResponse<VendorResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vendor = await _vendorRepository.GetActiveByIdAsync(id, cancellationToken);

        if (vendor is null)
        {
            return ApiResponseFactory.Fail<VendorResponse>("Vendor not found.");
        }

        return ApiResponseFactory.Ok("Vendor retrieved successfully.", vendor.ToResponse());
    }

    public async Task<ApiResponse<VendorResponse>> UpdateAsync(
        Guid id,
        UpdateVendorRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return ApiResponseFactory.FailFromValidation<VendorResponse>(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var vendor = await _vendorRepository.GetActiveByIdForUpdateAsync(id, cancellationToken);

        if (vendor is null)
        {
            return ApiResponseFactory.Fail<VendorResponse>("Vendor not found.");
        }

        var normalizedName = request.VendorName.Trim().ToLowerInvariant();
        var duplicateExists = await _vendorRepository.ExistsActiveByNameAsync(normalizedName, id, cancellationToken);

        if (duplicateExists)
        {
            return ApiResponseFactory.Fail<VendorResponse>("Duplicate vendor name is not allowed.");
        }

        var normalizedEmail = StringNormalizer.NormalizeOptional(request.Email);
        if (!string.IsNullOrWhiteSpace(normalizedEmail))
        {
            var duplicateEmail = await _vendorRepository.ExistsByEmailAsync(
                normalizedEmail.ToLowerInvariant(),
                id,
                cancellationToken);
            if (duplicateEmail)
            {
                return ApiResponseFactory.Fail<VendorResponse>("Vendor email already exists.");
            }
        }

        vendor.VendorName = request.VendorName.Trim();
        vendor.ContactPerson = StringNormalizer.NormalizeOptional(request.ContactPerson);
        vendor.Phone = request.Phone.Trim();
        vendor.Email = normalizedEmail;
        vendor.Address = StringNormalizer.NormalizeOptional(request.Address);
        vendor.UpdatedAt = DateTime.UtcNow;

        _vendorRepository.Update(vendor);
        await _vendorRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Vendor updated successfully.", vendor.ToResponse());
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vendor = await _vendorRepository.GetActiveByIdForUpdateAsync(id, cancellationToken);

        if (vendor is null)
        {
            return ApiResponseFactory.Fail<bool>("Vendor not found.");
        }

        vendor.IsActive = false;
        vendor.UpdatedAt = DateTime.UtcNow;

        _vendorRepository.Update(vendor);
        await _vendorRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Vendor deleted successfully.", true);
    }

    public async Task<ApiResponse<List<VendorResponse>>> SearchAsync(
        string? query,
        CancellationToken cancellationToken = default)
    {
        var vendors = await _vendorRepository.SearchActiveAsync(query, cancellationToken);
        return ApiResponseFactory.Ok("Vendors retrieved successfully.", vendors.Select(v => v.ToResponse()).ToList());
    }
}
