using AutoFlow_Backend.Application.DTOs.Staff;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Mappers;

public static class StaffMapper
{
    public static StaffResponse ToResponse(this Staff staff)
    {
        return new StaffResponse
        {
            Id = staff.Id,
            ApplicationUserId = staff.ApplicationUserId,
            StaffCode = staff.StaffCode,
            FullName = staff.FullName,
            Email = staff.Email,
            Phone = staff.PhoneNumber,
            Address = staff.Address,
            Position = staff.Position,
            IsActive = staff.IsActive,
            CreatedAt = staff.CreatedAt,
            UpdatedAt = staff.UpdatedAt
        };
    }
}