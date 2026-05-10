using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Vehicles;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Application.Mappers;
using AutoFlow_Backend.Domain.Entities;
using FluentValidation;

namespace AutoFlow_Backend.Application.Services;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IValidator<VehicleCreateDto> _createValidator;
    private readonly IValidator<VehicleUpdateDto> _updateValidator;

    public VehicleService(
        IVehicleRepository vehicleRepository,
        ICustomerRepository customerRepository,
        IValidator<VehicleCreateDto> createValidator,
        IValidator<VehicleUpdateDto> updateValidator)
    {
        _vehicleRepository = vehicleRepository;
        _customerRepository = customerRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ApiResponse<VehicleResponseDto>> CreateAsync(
        VehicleCreateDto request,
        Guid? creatorUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return ApiResponseFactory.FailFromValidation<VehicleResponseDto>(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        Guid targetUserId;

        if (isStaffOrAdmin && request.OwnerUserId.HasValue)
        {
            var customer = await _customerRepository.GetByApplicationUserIdAsync(request.OwnerUserId.Value, cancellationToken);
            if (customer is null)
                return ApiResponseFactory.Fail<VehicleResponseDto>("No customer found for the specified user.");
            targetUserId = request.OwnerUserId.Value;
        }
        else if (creatorUserId.HasValue)
        {
            targetUserId = creatorUserId.Value;
        }
        else
        {
            return ApiResponseFactory.Fail<VehicleResponseDto>("Unable to determine vehicle owner.");
        }

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            VehicleNumber = NormalizeVehicleNumber(request.VehicleNumber),
            Brand = request.Brand.Trim(),
            Model = request.Model.Trim(),
            Year = request.Year,
            Mileage = request.Mileage,
            Color = StringNormalizer.NormalizeOptional(request.Color),
            VIN = StringNormalizer.NormalizeOptional(request.VIN),
            UserId = targetUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _vehicleRepository.AddAsync(vehicle, cancellationToken);
        await _vehicleRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Vehicle created successfully.", vehicle.ToResponse());
    }

    public async Task<ApiResponse<List<VehicleResponseDto>>> GetAllAsync(
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default)
    {
        List<Vehicle> vehicles;

        if (isStaffOrAdmin)
        {
            vehicles = await _vehicleRepository.GetAllAsync(cancellationToken);
        }
        else if (requestingUserId.HasValue)
        {
            vehicles = await _vehicleRepository.GetByUserIdAsync(requestingUserId.Value, cancellationToken);
        }
        else
        {
            return ApiResponseFactory.Fail<List<VehicleResponseDto>>("Unable to determine user.");
        }

        return ApiResponseFactory.Ok("Vehicles retrieved successfully.", vehicles.Select(v => v.ToResponse()).ToList());
    }

    public async Task<ApiResponse<VehicleResponseDto>> GetByIdAsync(
        Guid id,
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id, cancellationToken);
        if (vehicle is null)
            return ApiResponseFactory.FailNotFound<VehicleResponseDto>("Vehicle not found.");

        if (!isStaffOrAdmin && (!requestingUserId.HasValue || vehicle.UserId != requestingUserId.Value))
            return ApiResponseFactory.FailNotFound<VehicleResponseDto>("Vehicle not found.");

        return ApiResponseFactory.Ok("Vehicle retrieved successfully.", vehicle.ToResponse());
    }

    public async Task<ApiResponse<List<VehicleResponseDto>>> GetMyVehiclesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var vehicles = await _vehicleRepository.GetByUserIdAsync(userId, cancellationToken);
        return ApiResponseFactory.Ok("My vehicles retrieved successfully.", vehicles.Select(v => v.ToResponse()).ToList());
    }

    public async Task<ApiResponse<VehicleResponseDto>> UpdateAsync(
        Guid id,
        VehicleUpdateDto request,
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return ApiResponseFactory.FailFromValidation<VehicleResponseDto>(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var vehicle = await _vehicleRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (vehicle is null)
            return ApiResponseFactory.FailNotFound<VehicleResponseDto>("Vehicle not found.");

        if (!isStaffOrAdmin && (!requestingUserId.HasValue || vehicle.UserId != requestingUserId.Value))
            return ApiResponseFactory.FailNotFound<VehicleResponseDto>("Vehicle not found.");

        vehicle.VehicleNumber = NormalizeVehicleNumber(request.VehicleNumber);
        vehicle.Brand = request.Brand.Trim();
        vehicle.Model = request.Model.Trim();
        vehicle.Year = request.Year;
        vehicle.Color = StringNormalizer.NormalizeOptional(request.Color);
        vehicle.VIN = StringNormalizer.NormalizeOptional(request.VIN);
        vehicle.UpdatedAt = DateTime.UtcNow;

        if (isStaffOrAdmin && request.Mileage.HasValue)
        {
            vehicle.Mileage = request.Mileage.Value;
        }

        _vehicleRepository.Update(vehicle);
        await _vehicleRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Vehicle updated successfully.", vehicle.ToResponse());
    }

    public async Task<ApiResponse<bool>> DeleteAsync(
        Guid id,
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicleRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (vehicle is null)
            return ApiResponseFactory.FailNotFound<bool>("Vehicle not found.");

        if (!isStaffOrAdmin && (!requestingUserId.HasValue || vehicle.UserId != requestingUserId.Value))
            return ApiResponseFactory.FailNotFound<bool>("Vehicle not found.");

        _vehicleRepository.Delete(vehicle);
        await _vehicleRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Vehicle deleted successfully.", true);
    }

    public async Task<List<Guid>> GetUserIdsBySearchQueryAsync(
        string normalizedQuery,
        CancellationToken cancellationToken = default)
    {
        return await _vehicleRepository.GetUserIdsByVehicleQueryAsync(normalizedQuery, cancellationToken);
    }

    private static string NormalizeVehicleNumber(string vehicleNumber) =>
        vehicleNumber.Trim().ToUpperInvariant();
}