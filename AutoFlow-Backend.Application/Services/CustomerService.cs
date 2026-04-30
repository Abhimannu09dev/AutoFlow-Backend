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
            return FailFromValidation<CustomerResponseDto>(validationErrors);
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        var emailExists = await _dbContext.Customers
            .AsNoTracking()
            .AnyAsync(customer => customer.Email.ToLower() == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            return Fail<CustomerResponseDto>("Email is already registered.");
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

        return Success("Customers retrieved successfully.", customers);
    }

    public async Task<ApiResponse<CustomerResponseDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
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
            return Fail<CustomerResponseDto>("Customer not found.");
        }

        return Success("Customer retrieved successfully.", customer);
    }

    public async Task<ApiResponse<CustomerResponseDto>> UpdateAsync(
        int id,
        CustomerUpdateDto request,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = Validate(request.FullName, request.Email, request.Phone, request.Address);
        if (validationErrors.Count > 0)
        {
            return FailFromValidation<CustomerResponseDto>(validationErrors);
        }

        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);

        if (customer is null)
        {
            return Fail<CustomerResponseDto>("Customer not found.");
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        var emailExists = await _dbContext.Customers
            .AsNoTracking()
            .AnyAsync(customer => customer.Id != id && customer.Email.ToLower() == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            return Fail<CustomerResponseDto>("Email is already registered.");
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
        => new() { Status = true, Message = message, Data = data };

    private static ApiResponse<T> Fail<T>(string message)
        => new() { Status = false, Message = message, Data = default };

    private static ApiResponse<T> FailFromValidation<T>(List<string> errors)
        => Fail<T>(errors.Count > 0 ? string.Join(" ", errors) : "Validation failed.");
}