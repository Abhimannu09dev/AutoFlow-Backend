using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Vehicles;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Services;

public class VehicleService : IVehicleService
{
    private const int VehicleNumberMaxLength = 20;
    private const int VehicleBrandMaxLength = 50;
    private const int VehicleModelMaxLength = 50;
    private const int VehicleColorMaxLength = 30;
    private const int VehicleVinMaxLength = 50;

    private readonly IVehicleRepository _vehicleRepository;
    private readonly ICustomerRepository _customerRepository;

    public VehicleService(
        IVehicleRepository vehicleRepository,
        ICustomerRepository customerRepository)
    {
        _vehicleRepository = vehicleRepository;
        _customerRepository = customerRepository;
    }

    public async Task<ApiResponse<VehicleResponseDto>> CreateAsync(
        VehicleCreateDto request,
        Guid? creatorUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = ValidateVehicle(request.VehicleNumber, request.Brand, request.Model, request.Year, request.Color, request.VIN);
        if (validationErrors.Count > 0)
            return ApiResponseFactory.FailFromValidation<VehicleResponseDto>(validationErrors);

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
            Color = NormalizeOptional(request.Color),
            VIN = NormalizeOptional(request.VIN),
            UserId = targetUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _vehicleRepository.AddAsync(vehicle, cancellationToken);
        await _vehicleRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Vehicle created successfully.", Map(vehicle));
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

        return ApiResponseFactory.Ok("Vehicles retrieved successfully.", vehicles.Select(Map).ToList());
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

        return ApiResponseFactory.Ok("Vehicle retrieved successfully.", Map(vehicle));
    }

    public async Task<ApiResponse<List<VehicleResponseDto>>> GetMyVehiclesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var vehicles = await _vehicleRepository.GetByUserIdAsync(userId, cancellationToken);
        return ApiResponseFactory.Ok("My vehicles retrieved successfully.", vehicles.Select(Map).ToList());
    }

    public async Task<ApiResponse<VehicleResponseDto>> UpdateAsync(
        Guid id,
        VehicleUpdateDto request,
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = ValidateVehicle(request.VehicleNumber, request.Brand, request.Model, request.Year, request.Color, request.VIN);
        if (validationErrors.Count > 0)
            return ApiResponseFactory.FailFromValidation<VehicleResponseDto>(validationErrors);

        var vehicle = await _vehicleRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (vehicle is null)
            return ApiResponseFactory.FailNotFound<VehicleResponseDto>("Vehicle not found.");

        if (!isStaffOrAdmin && (!requestingUserId.HasValue || vehicle.UserId != requestingUserId.Value))
            return ApiResponseFactory.FailNotFound<VehicleResponseDto>("Vehicle not found.");

        vehicle.VehicleNumber = NormalizeVehicleNumber(request.VehicleNumber);
        vehicle.Brand = request.Brand.Trim();
        vehicle.Model = request.Model.Trim();
        vehicle.Year = request.Year;
        vehicle.Color = NormalizeOptional(request.Color);
        vehicle.VIN = NormalizeOptional(request.VIN);
        vehicle.UpdatedAt = DateTime.UtcNow;

        if (isStaffOrAdmin && request.Mileage.HasValue)
        {
            vehicle.Mileage = request.Mileage.Value;
        }

        _vehicleRepository.Update(vehicle);
        await _vehicleRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Vehicle updated successfully.", Map(vehicle));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicleRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (vehicle is null)
            return ApiResponseFactory.FailNotFound<bool>("Vehicle not found.");

        _vehicleRepository.Delete(vehicle);
        await _vehicleRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Vehicle deleted successfully.", true);
    }

    private static VehicleResponseDto Map(Vehicle vehicle) => new()
    {
        Id = vehicle.Id,
        VehicleNumber = vehicle.VehicleNumber,
        Brand = vehicle.Brand,
        Model = vehicle.Model,
        Year = vehicle.Year,
        Mileage = vehicle.Mileage,
        Color = vehicle.Color,
        VIN = vehicle.VIN,
        UserId = vehicle.UserId,
        CreatedAt = vehicle.CreatedAt,
        UpdatedAt = vehicle.UpdatedAt
    };

    private static List<string> ValidateVehicle(
        string? vehicleNumber, string? brand, string? model,
        int year, string? color, string? vin)
    {
        var errors = new List<string>();
        var currentYear = DateTime.UtcNow.Year;

        if (string.IsNullOrWhiteSpace(vehicleNumber))
            errors.Add("Vehicle number is required.");
        else if (vehicleNumber.Trim().Length > VehicleNumberMaxLength)
            errors.Add($"Vehicle number must be at most {VehicleNumberMaxLength} characters.");

        if (string.IsNullOrWhiteSpace(brand))
            errors.Add("Brand is required.");
        else if (brand.Trim().Length > VehicleBrandMaxLength)
            errors.Add($"Brand must be at most {VehicleBrandMaxLength} characters.");

        if (string.IsNullOrWhiteSpace(model))
            errors.Add("Model is required.");
        else if (model.Trim().Length > VehicleModelMaxLength)
            errors.Add($"Model must be at most {VehicleModelMaxLength} characters.");

        if (year < 1886 || year > currentYear + 1)
            errors.Add($"Year must be between 1886 and {currentYear + 1}.");

        if (!string.IsNullOrWhiteSpace(color) && color.Trim().Length > VehicleColorMaxLength)
            errors.Add($"Color must be at most {VehicleColorMaxLength} characters.");

        if (!string.IsNullOrWhiteSpace(vin) && vin.Trim().Length > VehicleVinMaxLength)
            errors.Add($"VIN must be at most {VehicleVinMaxLength} characters.");

        return errors;
    }

    private static string NormalizeVehicleNumber(string vehicleNumber) =>
        vehicleNumber.Trim().ToUpperInvariant();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}