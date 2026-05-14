using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Staff;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Application.Mappers;
using FluentValidation;

namespace AutoFlow_Backend.Application.Services;

public class StaffSelfService : IStaffSelfService
{
    private readonly IStaffRepository _staffRepository;
    private readonly IIdentityService _identityService;
    private readonly IValidator<StaffPatchDto> _validator;

    public StaffSelfService(
        IStaffRepository staffRepository,
        IIdentityService identityService,
        IValidator<StaffPatchDto> validator)
    {
        _staffRepository = staffRepository;
        _identityService = identityService;
        _validator = validator;
    }

    public async Task<ApiResponse<StaffResponse>> GetMyProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var staff = await _staffRepository.GetByApplicationUserIdAsync(userId, cancellationToken);
        if (staff is null)
            return ApiResponseFactory.FailNotFound<StaffResponse>("Staff profile not found.");

        return ApiResponseFactory.Ok("Profile retrieved successfully.", staff.ToResponse());
    }

    public async Task<ApiResponse<StaffResponse>> UpdateMyProfileAsync(
        Guid userId,
        StaffPatchDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return ApiResponseFactory.FailFromValidation<StaffResponse>(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var staff = await _staffRepository.GetByApplicationUserIdAsync(userId, cancellationToken);
        if (staff is null)
            return ApiResponseFactory.FailNotFound<StaffResponse>("Staff profile not found.");

        var fullName = !string.IsNullOrWhiteSpace(request.FullName) ? request.FullName.Trim() : staff.FullName;
        var phone = request.Phone is not null ? StringNormalizer.NormalizeOptional(request.Phone) : staff.PhoneNumber;
        var address = request.Address is not null ? StringNormalizer.NormalizeOptional(request.Address) : staff.Address;

        await _identityService.UpdateUserAsync(
            userId: staff.ApplicationUserId.ToString(),
            email: staff.Email,
            fullName: fullName,
            phone: phone,
            address: address,
            cancellationToken: cancellationToken);

        staff.FullName = fullName;
        staff.PhoneNumber = phone;
        staff.Address = address;
        staff.UpdatedAt = DateTime.UtcNow;

        _staffRepository.Update(staff);
        await _staffRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Profile updated successfully.", staff.ToResponse());
    }
}
