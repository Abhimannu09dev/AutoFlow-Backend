using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Customers;
using AutoFlow_Backend.Application.DTOs.Vehicles;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Application.Services;

public class CustomerService : ICustomerService
{
    private const int FullNameMaxLength = 150;
    private const int EmailMaxLength = 200;
    private const int PhoneMaxLength = 30;
    private const int AddressMaxLength = 300;
    private const int VehicleNumberMaxLength = 20;
    private const int VehicleBrandMaxLength = 50;
    private const int VehicleModelMaxLength = 50;
    private const int VehicleColorMaxLength = 30;
    private const int VehicleVinMaxLength = 50;

    private readonly IAppDbContext _dbContext;

    public CustomerService(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<CustomerResponseDto>> CreateAsync(
        CustomerCreateDto request,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = Validate(request.FullName, request.Email, request.Phone, request.Address);
        if (validationErrors.Count > 0)
        {
            return ValidationFailure<CustomerResponseDto>(validationErrors);
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        var emailExists = await _dbContext.Customers
            .AsNoTracking()
            .AnyAsync(customer => customer.Email.ToLower() == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            return Failure<CustomerResponseDto>("Email is already registered.");
        }

        var customer = new Customer
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            Phone = NormalizeOptional(request.Phone),
            Address = NormalizeOptional(request.Address),
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Customers.AddAsync(customer, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Success("Customer created successfully.", Map(customer));
    }

    public async Task<ApiResponse<List<CustomerResponseDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _dbContext.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.FullName)
            .Select(customer => Map(customer))
            .ToListAsync(cancellationToken);

        return Success("Customers retrieved successfully.", customers);
    }

    public async Task<ApiResponse<List<CustomerResponseDto>>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return ValidationFailure<List<CustomerResponseDto>>(new List<string> { "Search query is required." });
        }

        var normalizedQuery = query.Trim();
        var normalizedLowerQuery = normalizedQuery.ToLowerInvariant();
        var customerIdMatch = Guid.TryParse(normalizedQuery, out var customerId);

        var matchingVehicleUserIds = await _dbContext.Vehicles
            .AsNoTracking()
            .Where(vehicle =>
                vehicle.VehicleNumber.ToLower().Contains(normalizedLowerQuery) ||
                vehicle.Brand.ToLower().Contains(normalizedLowerQuery) ||
                vehicle.Model.ToLower().Contains(normalizedLowerQuery))
            .Select(vehicle => vehicle.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var customers = await _dbContext.Customers
            .AsNoTracking()
            .Where(customer =>
                (customer.FullName.ToLower().Contains(normalizedLowerQuery)) ||
                (customer.Email.ToLower().Contains(normalizedLowerQuery)) ||
                (customer.Phone != null && customer.Phone.ToLower().Contains(normalizedLowerQuery)) ||
                (customerIdMatch && customer.Id == customerId) ||
                (customer.ApplicationUserId.HasValue && matchingVehicleUserIds.Contains(customer.ApplicationUserId.Value)))
            .OrderBy(customer => customer.FullName)
            .Select(customer => Map(customer))
            .ToListAsync(cancellationToken);

        return Success("Customers searched successfully.", customers);
    }

    public async Task<ApiResponse<CustomerResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers
            .AsNoTracking()
            .Where(customer => customer.Id == id)
            .Select(customer => Map(customer))
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            return Failure<CustomerResponseDto>("Customer not found.");
        }

        return Success("Customer retrieved successfully.", customer);
    }

    public async Task<ApiResponse<CustomerResponseDto>> UpdateAsync(
        Guid id,
        CustomerUpdateDto request,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = Validate(request.FullName, request.Email, request.Phone, request.Address);
        if (validationErrors.Count > 0)
        {
            return ValidationFailure<CustomerResponseDto>(validationErrors);
        }

        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);

        if (customer is null)
        {
            return Failure<CustomerResponseDto>("Customer not found.");
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        var emailExists = await _dbContext.Customers
            .AsNoTracking()
            .AnyAsync(customer => customer.Id != id && customer.Email.ToLower() == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            return Failure<CustomerResponseDto>("Email is already registered.");
        }

        customer.FullName = request.FullName.Trim();
        customer.Email = normalizedEmail;
        customer.Phone = NormalizeOptional(request.Phone);
        customer.Address = NormalizeOptional(request.Address);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Success("Customer updated successfully.", Map(customer));
    }

    public async Task<ApiResponse<VehicleResponseDto>> AddVehicleAsync(
        Guid customerId,
        VehicleCreateDto request,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = ValidateVehicle(request.VehicleNumber, request.Brand, request.Model, request.Year, request.Color, request.VIN);
        if (validationErrors.Count > 0)
        {
            return ValidationFailure<VehicleResponseDto>(validationErrors);
        }

        var customer = await _dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(customer => customer.Id == customerId, cancellationToken);

        if (customer is null)
        {
            return Failure<VehicleResponseDto>("Customer not found.");
        }

        if (customer.ApplicationUserId is null)
        {
            return Failure<VehicleResponseDto>("Customer is not linked to a user account.");
        }

        var vehicle = new Vehicle
        {
            VehicleNumber = NormalizeVehicleNumber(request.VehicleNumber),
            Brand = request.Brand.Trim(),
            Model = request.Model.Trim(),
            Year = request.Year,
            Color = NormalizeOptional(request.Color),
            VIN = NormalizeOptional(request.VIN),
            UserId = customer.ApplicationUserId.Value,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Vehicles.AddAsync(vehicle, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Success("Vehicle added successfully.", Map(vehicle));
    }

    public async Task<ApiResponse<List<VehicleResponseDto>>> GetVehiclesAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(customer => customer.Id == customerId, cancellationToken);

        if (customer is null)
        {
            return Failure<List<VehicleResponseDto>>("Customer not found.");
        }

        if (customer.ApplicationUserId is null)
        {
            return Failure<List<VehicleResponseDto>>("Customer is not linked to a user account.");
        }

        var vehicles = await _dbContext.Vehicles
            .AsNoTracking()
            .Where(vehicle => vehicle.UserId == customer.ApplicationUserId.Value)
            .OrderByDescending(vehicle => vehicle.CreatedAt)
            .Select(vehicle => Map(vehicle))
            .ToListAsync(cancellationToken);

        return Success("Customer vehicles retrieved successfully.", vehicles);
    }

    private static CustomerResponseDto Map(Customer customer)
    {
        return new CustomerResponseDto
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            CreatedAt = customer.CreatedAt
        };
    }

    private static VehicleResponseDto Map(Vehicle vehicle)
    {
        return new VehicleResponseDto
        {
            Id = vehicle.Id,
            VehicleNumber = vehicle.VehicleNumber,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Color = vehicle.Color,
            VIN = vehicle.VIN,
            UserId = vehicle.UserId,
            CreatedAt = vehicle.CreatedAt,
            UpdatedAt = vehicle.UpdatedAt
        };
    }

    private static List<string> Validate(string? fullName, string? email, string? phone, string? address)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            errors.Add("Full name is required.");
        }
        else if (fullName.Trim().Length > FullNameMaxLength)
        {
            errors.Add($"Full name must be at most {FullNameMaxLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add("Email is required.");
        }
        else if (email.Trim().Length > EmailMaxLength)
        {
            errors.Add($"Email must be at most {EmailMaxLength} characters.");
        }

        if (!string.IsNullOrWhiteSpace(phone) && phone.Trim().Length > PhoneMaxLength)
        {
            errors.Add($"Phone must be at most {PhoneMaxLength} characters.");
        }

        if (!string.IsNullOrWhiteSpace(address) && address.Trim().Length > AddressMaxLength)
        {
            errors.Add($"Address must be at most {AddressMaxLength} characters.");
        }

        return errors;
    }

    private static List<string> ValidateVehicle(
        string? vehicleNumber,
        string? brand,
        string? model,
        int year,
        string? color,
        string? vin)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(vehicleNumber))
        {
            errors.Add("Vehicle number is required.");
        }
        else if (vehicleNumber.Trim().Length > VehicleNumberMaxLength)
        {
            errors.Add($"Vehicle number must be at most {VehicleNumberMaxLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(brand))
        {
            errors.Add("Brand is required.");
        }
        else if (brand.Trim().Length > VehicleBrandMaxLength)
        {
            errors.Add($"Brand must be at most {VehicleBrandMaxLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            errors.Add("Model is required.");
        }
        else if (model.Trim().Length > VehicleModelMaxLength)
        {
            errors.Add($"Model must be at most {VehicleModelMaxLength} characters.");
        }

        var currentYear = DateTime.UtcNow.Year;
        if (year < 1886 || year > currentYear + 1)
        {
            errors.Add($"Year must be between 1886 and {currentYear + 1}.");
        }

        if (!string.IsNullOrWhiteSpace(color) && color.Trim().Length > VehicleColorMaxLength)
        {
            errors.Add($"Color must be at most {VehicleColorMaxLength} characters.");
        }

        if (!string.IsNullOrWhiteSpace(vin) && vin.Trim().Length > VehicleVinMaxLength)
        {
            errors.Add($"VIN must be at most {VehicleVinMaxLength} characters.");
        }

        return errors;
    }

    private static string NormalizeVehicleNumber(string vehicleNumber)
        => vehicleNumber.Trim().ToUpperInvariant();

    private static string NormalizeEmail(string email)
        => email.Trim().ToLowerInvariant();

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ApiResponse<T> Success<T>(string message, T data)
        => new()
        {
            Status = true,
            Message = message,
            Data = data
        };

    private static ApiResponse<T> Failure<T>(string message)
        => new()
        {
            Status = false,
            Message = message,
            Data = default
        };

    private static ApiResponse<T> ValidationFailure<T>(List<string> errors)
        => Failure<T>(errors.Count > 0 ? string.Join(" ", errors) : "Validation failed.");
}
