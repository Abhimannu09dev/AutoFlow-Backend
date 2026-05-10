using AutoFlow_Backend.Application.DTOs.Vendors;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Mappers;

public static class VendorMapper
{
    public static VendorResponse ToResponse(this Vendor vendor)
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
}