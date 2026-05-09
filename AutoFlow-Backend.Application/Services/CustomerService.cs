using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Appointments;
using AutoFlow_Backend.Application.DTOs.Customers;
using AutoFlow_Backend.Application.DTOs.Sales;
using AutoFlow_Backend.Application.DTOs.Vehicles;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AutoFlow_Backend.Application.Services;

public class CustomerService : ICustomerService
{
    private const int FullNameMaxLength = 150;
    private const int EmailMaxLength = 200;
    private const int PhoneMaxLength = 30;
    private const int AddressMaxLength = 300;

    private readonly ICustomerRepository _customerRepository;
    private readonly IVehicleService _vehicleService;
    private readonly ISaleRepository _saleRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IIdentityService _identityService;
    private readonly ICustomerAccountService _customerAccountService;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICustomerRepository customerRepository,
        IVehicleService vehicleService,
        ISaleRepository saleRepository,
        IAppointmentRepository appointmentRepository,
        IIdentityService identityService,
        ICustomerAccountService customerAccountService,
        ILogger<CustomerService> logger)
    {
        _customerRepository = customerRepository;
        _vehicleService = vehicleService;
        _saleRepository = saleRepository;
        _appointmentRepository = appointmentRepository;
        _identityService = identityService;
        _customerAccountService = customerAccountService;
        _logger = logger;
    }

    public async Task<ApiResponse<CustomerResponseDto>> CreateAsync(
        CustomerCreateDto request,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = Validate(request.FullName, request.Email, request.Phone, request.Address);
        if (validationErrors.Count > 0)
            return ApiResponseFactory.FailFromValidation<CustomerResponseDto>(validationErrors);

        var normalizedEmail = NormalizeEmail(request.Email);

        var emailExistsInUsers = await _identityService.UserExistsByEmailAsync(normalizedEmail, null, cancellationToken);
        if (emailExistsInUsers)
            return ApiResponseFactory.FailConflict<CustomerResponseDto>("Email already exists as a user account.");

        var emailExistsInCustomers = await _customerRepository.EmailExistsAsync(normalizedEmail, null, cancellationToken);
        if (emailExistsInCustomers)
            return ApiResponseFactory.FailConflict<CustomerResponseDto>("Customer with this email already exists.");

        Guid? applicationUserId = null;
        var message = "Customer created successfully.";

        if (request.CreateLoginAccount)
        {
            var provisionResult = await _customerAccountService.ProvisionAsync(
                request.FullName.Trim(),
                normalizedEmail,
                NormalizeOptional(request.Phone),
                NormalizeOptional(request.Address),
                cancellationToken);

            if (!provisionResult.IsSuccess)
                return ApiResponseFactory.Fail<CustomerResponseDto>(provisionResult.Message);

            applicationUserId = provisionResult.Data!.ApplicationUserId;
            message = provisionResult.Data.WelcomeMessage;
        }

        var customer = new Customer
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            Phone = NormalizeOptional(request.Phone),
            Address = NormalizeOptional(request.Address),
            ApplicationUserId = applicationUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        if (request.Vehicle is not null && applicationUserId.HasValue)
        {
            var vehicleResponse = await _vehicleService.CreateAsync(
                request.Vehicle,
                creatorUserId: applicationUserId,
                isStaffOrAdmin: false,
                cancellationToken);

            if (vehicleResponse.IsSuccess)
            {
                message += " Vehicle created successfully.";
            }
            else
            {
                message += " Vehicle creation skipped: " + vehicleResponse.Message;
            }
        }
        else if (request.Vehicle is not null && !applicationUserId.HasValue)
        {
            message += " Vehicle not created: Customer does not have a linked user account.";
        }

        return ApiResponseFactory.Ok(message, Map(customer));
    }

    public async Task<ApiResponse<List<CustomerResponseDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetAllAsync(cancellationToken);
        return ApiResponseFactory.Ok("Customers retrieved successfully.", customers.Select(Map).ToList());
    }

    public async Task<ApiResponse<CustomerResponseDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customer is null)
            return ApiResponseFactory.FailNotFound<CustomerResponseDto>("Customer not found.");

        return ApiResponseFactory.Ok("Customer retrieved successfully.", Map(customer));
    }

    public async Task<ApiResponse<CustomerResponseDto>> UpdateAsync(
        Guid id,
        CustomerUpdateDto request,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = Validate(request.FullName, request.Email, request.Phone, request.Address);
        if (validationErrors.Count > 0)
            return ApiResponseFactory.FailFromValidation<CustomerResponseDto>(validationErrors);

        var customer = await _customerRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (customer is null)
            return ApiResponseFactory.FailNotFound<CustomerResponseDto>("Customer not found.");

        var normalizedEmail = NormalizeEmail(request.Email);
        var emailExists = await _customerRepository.EmailExistsAsync(normalizedEmail, id, cancellationToken);
        if (emailExists)
            return ApiResponseFactory.FailConflict<CustomerResponseDto>("Email is already registered.");

        customer.FullName = request.FullName.Trim();
        customer.Email = normalizedEmail;
        customer.Phone = NormalizeOptional(request.Phone);
        customer.Address = NormalizeOptional(request.Address);

        _customerRepository.Update(customer);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Customer updated successfully.", Map(customer));
    }

    public async Task<ApiResponse<List<CustomerResponseDto>>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return ApiResponseFactory.FailFromValidation<List<CustomerResponseDto>>(
                new List<string> { "Search query is required." });

        var normalizedLower = query.Trim().ToLowerInvariant();
        var customerIdMatch = Guid.TryParse(query.Trim(), out var parsedId) ? parsedId : (Guid?)null;

        var matchingUserIds = await _vehicleService.GetUserIdsBySearchQueryAsync(normalizedLower, cancellationToken);
        var customers = await _customerRepository.SearchAsync(normalizedLower, matchingUserIds, customerIdMatch, cancellationToken);

        return ApiResponseFactory.Ok("Customers searched successfully.", customers.Select(Map).ToList());
    }

    public async Task<ApiResponse<List<SaleResponse>>> GetPurchasesAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
            return ApiResponseFactory.FailNotFound<List<SaleResponse>>("Customer not found.");

        var purchases = await _saleRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        return ApiResponseFactory.Ok("Customer purchases retrieved successfully.", purchases.Select(MapSale).ToList());
    }

    public async Task<ApiResponse<List<AppointmentResponse>>> GetServicesAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
            return ApiResponseFactory.FailNotFound<List<AppointmentResponse>>("Customer not found.");

        var services = await _appointmentRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        return ApiResponseFactory.Ok("Customer services retrieved successfully.", services.Select(MapAppointment).ToList());
    }

    public async Task<ApiResponse<VehicleResponseDto>> AddVehicleAsync(
        Guid customerId,
        VehicleCreateDto request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
            return ApiResponseFactory.FailNotFound<VehicleResponseDto>("Customer not found.");

        if (customer.ApplicationUserId is null)
            return ApiResponseFactory.Fail<VehicleResponseDto>("Customer is not linked to a user account.");

        request.OwnerUserId = customer.ApplicationUserId;

        return await _vehicleService.CreateAsync(request, creatorUserId: null, isStaffOrAdmin: true, cancellationToken);
    }

    public async Task<ApiResponse<List<VehicleResponseDto>>> GetVehiclesAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
            return ApiResponseFactory.FailNotFound<List<VehicleResponseDto>>("Customer not found.");

        if (customer.ApplicationUserId is null)
            return ApiResponseFactory.Fail<List<VehicleResponseDto>>("Customer is not linked to a user account.");

        return await _vehicleService.GetMyVehiclesAsync(customer.ApplicationUserId.Value, cancellationToken);
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

    private static List<string> Validate(string? fullName, string? email, string? phone, string? address)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(fullName))
            errors.Add("Full name is required.");
        else if (fullName.Trim().Length > FullNameMaxLength)
            errors.Add($"Full name must be at most {FullNameMaxLength} characters.");

        if (string.IsNullOrWhiteSpace(email))
            errors.Add("Email is required.");
        else if (email.Trim().Length > EmailMaxLength)
            errors.Add($"Email must be at most {EmailMaxLength} characters.");

        if (!string.IsNullOrWhiteSpace(phone) && phone.Trim().Length > PhoneMaxLength)
            errors.Add($"Phone must be at most {PhoneMaxLength} characters.");

        if (!string.IsNullOrWhiteSpace(address) && address.Trim().Length > AddressMaxLength)
            errors.Add($"Address must be at most {AddressMaxLength} characters.");

        return errors;
    }

    private static string NormalizeVehicleNumber(string vehicleNumber) =>
        vehicleNumber.Trim().ToUpperInvariant();

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}