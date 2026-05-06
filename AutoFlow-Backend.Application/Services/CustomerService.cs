using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Appointments;
using AutoFlow_Backend.Application.DTOs.Customers;
using AutoFlow_Backend.Application.DTOs.Sales;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Application.Services;

public class CustomerService : ICustomerService
{
    private const int FullNameMaxLength = 150;
    private const int EmailMaxLength = 200;
    private const int PhoneMaxLength = 30;
    private const int AddressMaxLength = 300;

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

    public async Task<ApiResponse<List<SaleResponse>>> GetPurchasesAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customerExists = await _dbContext.Customers
            .AsNoTracking()
            .AnyAsync(customer => customer.Id == customerId, cancellationToken);

        if (!customerExists)
        {
            return Failure<List<SaleResponse>>("Customer not found.");
        }

        var purchases = await _dbContext.Sales
            .AsNoTracking()
            .Where(sale => sale.CustomerId == customerId)
            .OrderByDescending(sale => sale.SaleDate)
            .ThenByDescending(sale => sale.CreatedAt)
            .Select(sale => MapSale(sale))
            .ToListAsync(cancellationToken);

        return Success("Customer purchases retrieved successfully.", purchases);
    }

    public async Task<ApiResponse<List<AppointmentResponse>>> GetServicesAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customerExists = await _dbContext.Customers
            .AsNoTracking()
            .AnyAsync(customer => customer.Id == customerId, cancellationToken);

        if (!customerExists)
        {
            return Failure<List<AppointmentResponse>>("Customer not found.");
        }

        var services = await _dbContext.Appointments
            .AsNoTracking()
            .Where(appointment => appointment.CustomerId == customerId)
            .OrderByDescending(appointment => appointment.Date)
            .ThenByDescending(appointment => appointment.Time)
            .Select(appointment => MapAppointment(appointment))
            .ToListAsync(cancellationToken);

        return Success("Customer services retrieved successfully.", services);
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

    private static SaleResponse MapSale(Sale sale)
    {
        return new SaleResponse
        {
            Id = sale.Id,
            CustomerId = sale.CustomerId,
            CustomerName = sale.Customer?.FullName ?? string.Empty,
            StaffId = sale.StaffId,
            SaleDate = sale.SaleDate,
            SubTotal = sale.SubTotal,
            DiscountAmount = sale.DiscountAmount,
            TotalAmount = sale.TotalAmount,
            LoyaltyDiscountApplied = sale.DiscountAmount > 0,
            PaymentMethod = sale.PaymentMethod,
            Status = sale.Status,
            Notes = sale.Notes,
            CreatedAt = sale.CreatedAt,
            Items = sale.SaleItems.Select(item => new SaleItemResponse
            {
                Id = item.Id,
                PartId = item.PartId,
                PartName = item.Part?.PartName ?? string.Empty,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                SubTotal = item.SubTotal
            }).ToList()
        };
    }

    private static AppointmentResponse MapAppointment(Appointment appointment)
    {
        return new AppointmentResponse
        {
            Id = appointment.Id,
            CustomerId = appointment.CustomerId,
            Date = appointment.Date,
            Time = appointment.Time,
            Status = appointment.Status,
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt
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
