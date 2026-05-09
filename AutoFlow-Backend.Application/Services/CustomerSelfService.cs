using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Appointments;
using AutoFlow_Backend.Application.DTOs.Customers;
using AutoFlow_Backend.Application.DTOs.Sales;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Services;

public class CustomerSelfService : ICustomerSelfService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public CustomerSelfService(
        ICustomerRepository customerRepository,
        ISaleRepository saleRepository,
        IAppointmentRepository appointmentRepository)
    {
        _customerRepository = customerRepository;
        _saleRepository = saleRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<ApiResponse<List<SaleResponse>>> GetMyPurchasesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByApplicationUserIdAsync(userId, cancellationToken);
        if (customer is null)
            return ApiResponseFactory.FailNotFound<List<SaleResponse>>("Customer profile not found.");

        var purchases = await _saleRepository.GetByCustomerIdAsync(customer.Id, cancellationToken);
        return ApiResponseFactory.Ok("Your purchase history retrieved successfully.", purchases.Select(MapSale).ToList());
    }

    public async Task<ApiResponse<List<AppointmentResponse>>> GetMyServicesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByApplicationUserIdAsync(userId, cancellationToken);
        if (customer is null)
            return ApiResponseFactory.FailNotFound<List<AppointmentResponse>>("Customer profile not found.");

        var services = await _appointmentRepository.GetByCustomerIdAsync(customer.Id, cancellationToken);
        return ApiResponseFactory.Ok("Your service history retrieved successfully.", services.Select(MapAppointment).ToList());
    }

    public async Task<ApiResponse<CustomerResponseDto>> GetMyProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByApplicationUserIdAsync(userId, cancellationToken);
        if (customer is null)
            return ApiResponseFactory.FailNotFound<CustomerResponseDto>("Customer profile not found.");

        return ApiResponseFactory.Ok("Profile retrieved successfully.", Map(customer));
    }

    public async Task<ApiResponse<CustomerResponseDto>> UpdateMyProfileAsync(
        Guid userId,
        CustomerPatchDto request,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = ValidatePatch(request);
        if (validationErrors.Count > 0)
            return ApiResponseFactory.FailFromValidation<CustomerResponseDto>(validationErrors);

        var customer = await _customerRepository.GetByApplicationUserIdForUpdateAsync(userId, cancellationToken);
        if (customer is null)
            return ApiResponseFactory.FailNotFound<CustomerResponseDto>("Customer profile not found.");

        if (!string.IsNullOrWhiteSpace(request.FullName))
            customer.FullName = request.FullName.Trim();

        if (request.Phone is not null)
            customer.Phone = NormalizeOptional(request.Phone);

        if (request.Address is not null)
            customer.Address = NormalizeOptional(request.Address);

        _customerRepository.Update(customer);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Profile updated successfully.", Map(customer));
    }

    private static CustomerResponseDto Map(Customer customer) => new()
    {
        Id = customer.Id,
        FullName = customer.FullName,
        Email = customer.Email,
        Phone = customer.Phone,
        Address = customer.Address,
        CreatedAt = customer.CreatedAt,
        ApplicationUserId = customer.ApplicationUserId
    };

    private static SaleResponse MapSale(Sale sale) => new()
    {
        Id = sale.Id,
        InvoiceNumber = sale.InvoiceNumber,
        CustomerId = sale.CustomerId,
        SaleDate = sale.SaleDate,
        SubTotal = sale.SubTotal,
        DiscountAmount = sale.DiscountAmount,
        TotalAmount = sale.TotalAmount,
        LoyaltyDiscountApplied = sale.DiscountAmount > 0,
        PaymentMethod = sale.PaymentMethod,
        Status = sale.Status,
        CreatedAt = sale.CreatedAt
    };

    private static AppointmentResponse MapAppointment(Appointment appointment) => new()
    {
        Id = appointment.Id,
        CustomerId = appointment.CustomerId,
        Date = appointment.Date,
        Time = appointment.Time,
        Status = appointment.Status,
        CreatedAt = appointment.CreatedAt,
        UpdatedAt = appointment.UpdatedAt
    };

    private const int FullNameMaxLength = 150;
    private const int PhoneMaxLength = 30;
    private const int AddressMaxLength = 300;

    private static List<string> ValidatePatch(CustomerPatchDto request)
    {
        var errors = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.FullName) && request.FullName.Trim().Length > FullNameMaxLength)
            errors.Add($"Full name must be at most {FullNameMaxLength} characters.");

        if (!string.IsNullOrWhiteSpace(request.Phone) && request.Phone.Trim().Length > PhoneMaxLength)
            errors.Add($"Phone must be at most {PhoneMaxLength} characters.");

        if (!string.IsNullOrWhiteSpace(request.Address) && request.Address.Trim().Length > AddressMaxLength)
            errors.Add($"Address must be at most {AddressMaxLength} characters.");

        return errors;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}