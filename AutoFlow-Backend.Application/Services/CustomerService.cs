using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Customers;
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
    private const int VehicleModelMaxLength = 50;
    private const int VehicleBrandMaxLength = 50;

    private readonly IAppDbContext _dbContext;

    public CustomerService(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<APIResponse> CreateAsync(
        CustomerCreateDto request,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = Validate(request.FullName, request.Email, request.Phone, request.Address);
        if (validationErrors.Count > 0)
        {
            return ValidationFailure(validationErrors);
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        var emailExists = await _dbContext.Customers
            .AsNoTracking()
            .AnyAsync(customer => customer.Email.ToLower() == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            return Failure("Email is already registered.", 400);
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

        return Success("Customer created successfully.", Map(customer), 201);
    }

    public async Task<APIResponse> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _dbContext.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.FullName)
            .Select(customer => new CustomerResponseDto
            {
                Id = customer.Id,
                FullName = customer.FullName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                CreatedAt = customer.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Success("Customers retrieved successfully.", customers, 200);
    }

    public async Task<APIResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers
            .AsNoTracking()
            .Where(customer => customer.Id == id)
            .Select(customer => new CustomerResponseDto
            {
                Id = customer.Id,
                FullName = customer.FullName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                CreatedAt = customer.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            return Failure("Customer not found.", 404);
        }

        return Success("Customer retrieved successfully.", customer, 200);
    }

    public async Task<APIResponse> UpdateAsync(
        Guid id,
        CustomerUpdateDto request,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = Validate(request.FullName, request.Email, request.Phone, request.Address);
        if (validationErrors.Count > 0)
        {
            return ValidationFailure(validationErrors);
        }

        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);

        if (customer is null)
        {
            return Failure("Customer not found.", 404);
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        var emailExists = await _dbContext.Customers
            .AsNoTracking()
            .AnyAsync(customer => customer.Id != id && customer.Email.ToLower() == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            return Failure("Email is already registered.", 400);
        }

        customer.FullName = request.FullName.Trim();
        customer.Email = normalizedEmail;
        customer.Phone = NormalizeOptional(request.Phone);
        customer.Address = NormalizeOptional(request.Address);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Success("Customer updated successfully.", Map(customer), 200);
    }

    public async Task<APIResponse> AddVehicleAsync(
        Guid id,
        VehicleCreateDto request,
        CancellationToken cancellationToken = default)
    {
        var customerExists = await _dbContext.Customers
            .AsNoTracking()
            .AnyAsync(customer => customer.Id == id, cancellationToken);

        if (!customerExists)
        {
            return Failure("Customer not found.", 404);
        }

        var validationErrors = ValidateVehicle(request.VehicleNumber, request.Model, request.Brand);
        if (validationErrors.Count > 0)
        {
            return ValidationFailure(validationErrors);
        }

        var vehicle = new Vehicle
        {
            CustomerId = id,
            VehicleNumber = request.VehicleNumber.Trim(),
            Model = NormalizeOptional(request.Model),
            Brand = NormalizeOptional(request.Brand),
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Vehicles.AddAsync(vehicle, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Success("Vehicle added successfully.", Map(vehicle), 201);
    }

    public async Task<APIResponse> GetVehiclesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customerExists = await _dbContext.Customers
            .AsNoTracking()
            .AnyAsync(customer => customer.Id == id, cancellationToken);

        if (!customerExists)
        {
            return Failure("Customer not found.", 404);
        }

        var vehicles = await _dbContext.Vehicles
            .AsNoTracking()
            .Where(vehicle => vehicle.CustomerId == id)
            .OrderByDescending(vehicle => vehicle.CreatedAt)
            .Select(vehicle => new VehicleResponseDto
            {
                Id = vehicle.Id,
                CustomerId = vehicle.CustomerId,
                VehicleNumber = vehicle.VehicleNumber,
                Model = vehicle.Model,
                Brand = vehicle.Brand,
                CreatedAt = vehicle.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Success("Vehicles retrieved successfully.", vehicles, 200);
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
            CustomerId = vehicle.CustomerId,
            VehicleNumber = vehicle.VehicleNumber,
            Model = vehicle.Model,
            Brand = vehicle.Brand,
            CreatedAt = vehicle.CreatedAt
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

    private static List<string> ValidateVehicle(string? vehicleNumber, string? model, string? brand)
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

        if (!string.IsNullOrWhiteSpace(model) && model.Trim().Length > VehicleModelMaxLength)
        {
            errors.Add($"Model must be at most {VehicleModelMaxLength} characters.");
        }

        if (!string.IsNullOrWhiteSpace(brand) && brand.Trim().Length > VehicleBrandMaxLength)
        {
            errors.Add($"Brand must be at most {VehicleBrandMaxLength} characters.");
        }

        return errors;
    }

    private static string NormalizeEmail(string email)
        => email.Trim().ToLowerInvariant();

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static APIResponse Success(string message, object data, int statusCode)
        => new()
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = statusCode
        };

    private static APIResponse Failure(string message, int statusCode, List<string>? errors = null)
        => new()
        {
            Success = false,
            Message = message,
            Data = null,
            StatusCode = statusCode,
            Errors = errors
        };

    private static APIResponse ValidationFailure(List<string> errors)
        => Failure("Validation failed.", 400, errors);
}