using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Vendors;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Services;

public class VendorService : IVendorService
{
    private const int VendorNameMaxLength = 150;
    private const int ContactPersonMaxLength = 100;
    private const int PhoneMaxLength = 20;
    private const int EmailMaxLength = 200;
    private const int AddressMaxLength = 300;

    private readonly IVendorRepository _vendorRepository;

    public VendorService(IVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public async Task<ApiResponse<VendorResponse>> CreateAsync(
        CreateVendorRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = ValidateCreateOrUpdate(request.VendorName, request.Phone, request.Email, request.ContactPerson, request.Address);
        if (errors.Count > 0)
        {
            return FailFromValidation<VendorResponse>(errors);
        }

        var normalizedName = request.VendorName.Trim().ToLowerInvariant();
        var duplicateExists = await _vendorRepository.ExistsActiveByNameAsync(normalizedName, null, cancellationToken);

        if (duplicateExists)
        {
            return Fail<VendorResponse>("Duplicate vendor name is not allowed.");
        }

        var vendor = new Vendor
        {
            Id = Guid.NewGuid(),
            VendorName = request.VendorName.Trim(),
            ContactPerson = NormalizeOptional(request.ContactPerson),
            Phone = request.Phone.Trim(),
            Email = NormalizeOptional(request.Email),
            Address = NormalizeOptional(request.Address),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _vendorRepository.AddAsync(vendor, cancellationToken);
        await _vendorRepository.SaveChangesAsync(cancellationToken);

        return Success("Vendor created successfully.", Map(vendor));
    }

    public async Task<ApiResponse<List<VendorResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var vendors = await _vendorRepository.GetActiveAsync(cancellationToken);
        return Success("Vendors retrieved successfully.", vendors.Select(Map).ToList());
    }

    public async Task<ApiResponse<VendorResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vendor = await _vendorRepository.GetActiveByIdAsync(id, cancellationToken);

        if (vendor is null)
        {
            return Fail<VendorResponse>("Vendor not found.");
        }

        return Success("Vendor retrieved successfully.", Map(vendor));
    }

    public async Task<ApiResponse<VendorResponse>> UpdateAsync(
        Guid id,
        UpdateVendorRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = ValidateCreateOrUpdate(request.VendorName, request.Phone, request.Email, request.ContactPerson, request.Address);
        if (errors.Count > 0)
        {
            return FailFromValidation<VendorResponse>(errors);
        }

        var vendor = await _vendorRepository.GetActiveByIdForUpdateAsync(id, cancellationToken);

        if (vendor is null)
        {
            return Fail<VendorResponse>("Vendor not found.");
        }

        var normalizedName = request.VendorName.Trim().ToLowerInvariant();
        var duplicateExists = await _vendorRepository.ExistsActiveByNameAsync(normalizedName, id, cancellationToken);

        if (duplicateExists)
        {
            return Fail<VendorResponse>("Duplicate vendor name is not allowed.");
        }

        vendor.VendorName = request.VendorName.Trim();
        vendor.ContactPerson = NormalizeOptional(request.ContactPerson);
        vendor.Phone = request.Phone.Trim();
        vendor.Email = NormalizeOptional(request.Email);
        vendor.Address = NormalizeOptional(request.Address);
        vendor.UpdatedAt = DateTime.UtcNow;

        _vendorRepository.Update(vendor);
        await _vendorRepository.SaveChangesAsync(cancellationToken);

        return Success("Vendor updated successfully.", Map(vendor));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vendor = await _vendorRepository.GetActiveByIdForUpdateAsync(id, cancellationToken);

        if (vendor is null)
        {
            return Fail<bool>("Vendor not found.");
        }

        vendor.IsActive = false;
        vendor.UpdatedAt = DateTime.UtcNow;

        _vendorRepository.Update(vendor);
        await _vendorRepository.SaveChangesAsync(cancellationToken);

        return Success("Vendor deleted successfully.", true);
    }

    public async Task<ApiResponse<List<VendorResponse>>> SearchAsync(
        string? query,
        CancellationToken cancellationToken = default)
    {
        var vendors = await _vendorRepository.SearchActiveAsync(query, cancellationToken);
        return Success("Vendors retrieved successfully.", vendors.Select(Map).ToList());
    }

    private static VendorResponse Map(Vendor vendor)
    {
        return new VendorResponse
        {
            Id = vendor.Id,
            VendorName = vendor.VendorName,
            ContactPerson = vendor.ContactPerson,
            Phone = vendor.Phone,
            Email = vendor.Email,
            Address = vendor.Address,
            IsActive = vendor.IsActive,
            CreatedAt = vendor.CreatedAt,
            UpdatedAt = vendor.UpdatedAt
        };
    }

    private static List<string> ValidateCreateOrUpdate(
        string? vendorName,
        string? phone,
        string? email,
        string? contactPerson,
        string? address)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(vendorName))
        {
            errors.Add("Vendor name is required.");
        }
        else if (vendorName.Trim().Length > VendorNameMaxLength)
        {
            errors.Add($"Vendor name must be at most {VendorNameMaxLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            errors.Add("Phone is required.");
        }
        else if (phone.Trim().Length > PhoneMaxLength)
        {
            errors.Add($"Phone must be at most {PhoneMaxLength} characters.");
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            if (email.Trim().Length > EmailMaxLength)
            {
                errors.Add($"Email must be at most {EmailMaxLength} characters.");
            }
            else if (!IsValidEmail(email.Trim()))
            {
                errors.Add("Email must be valid.");
            }
        }

        if (!string.IsNullOrWhiteSpace(contactPerson) && contactPerson.Trim().Length > ContactPersonMaxLength)
        {
            errors.Add($"Contact person must be at most {ContactPersonMaxLength} characters.");
        }

        if (!string.IsNullOrWhiteSpace(address) && address.Trim().Length > AddressMaxLength)
        {
            errors.Add($"Address must be at most {AddressMaxLength} characters.");
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

    private static bool IsValidEmail(string email)
    {
        try
        {
            var _ = new System.Net.Mail.MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
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
