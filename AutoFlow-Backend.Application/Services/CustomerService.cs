using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Appointments;
using AutoFlow_Backend.Application.DTOs.Customers;
using AutoFlow_Backend.Application.DTOs.Sales;
using AutoFlow_Backend.Application.DTOs.Vehicles;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Application.Mappers;
using AutoFlow_Backend.Domain.Entities;
using FluentValidation;
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
    private readonly IValidator<CustomerCreateDto> _createValidator;
    private readonly IValidator<CustomerUpdateDto> _updateValidator;

    public CustomerService(
        ICustomerRepository customerRepository,
        IVehicleService vehicleService,
        ISaleRepository saleRepository,
        IAppointmentRepository appointmentRepository,
        IIdentityService identityService,
        ICustomerAccountService customerAccountService,
        ILogger<CustomerService> logger,
        IValidator<CustomerCreateDto> createValidator,
        IValidator<CustomerUpdateDto> updateValidator)
    {
        _customerRepository = customerRepository;
        _vehicleService = vehicleService;
        _saleRepository = saleRepository;
        _appointmentRepository = appointmentRepository;
        _identityService = identityService;
        _customerAccountService = customerAccountService;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ApiResponse<CustomerResponseDto>> CreateAsync(
        CustomerCreateDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return ApiResponseFactory.FailFromValidation<CustomerResponseDto>(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var normalizedEmail = StringNormalizer.NormalizeEmail(request.Email);

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
                StringNormalizer.NormalizeOptional(request.Phone),
                StringNormalizer.NormalizeOptional(request.Address),
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
            Phone = StringNormalizer.NormalizeOptional(request.Phone),
            Address = StringNormalizer.NormalizeOptional(request.Address),
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

        return ApiResponseFactory.Ok(message, CustomerMapper.ToResponse(customer));
    }

    public async Task<ApiResponse<List<CustomerResponseDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetAllAsync(cancellationToken);
        return ApiResponseFactory.Ok("Customers retrieved successfully.", customers.Select(CustomerMapper.ToResponse).ToList());
    }

    public async Task<ApiResponse<CustomerResponseDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customer is null)
            return ApiResponseFactory.FailNotFound<CustomerResponseDto>("Customer not found.");

        return ApiResponseFactory.Ok("Customer retrieved successfully.", CustomerMapper.ToResponse(customer));
    }

    public async Task<ApiResponse<CustomerResponseDto>> UpdateAsync(
        Guid id,
        CustomerUpdateDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return ApiResponseFactory.FailFromValidation<CustomerResponseDto>(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var customer = await _customerRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (customer is null)
            return ApiResponseFactory.FailNotFound<CustomerResponseDto>("Customer not found.");

        var normalizedEmail = StringNormalizer.NormalizeEmail(request.Email);
        var emailExists = await _customerRepository.EmailExistsAsync(normalizedEmail, id, cancellationToken);
        if (emailExists)
            return ApiResponseFactory.FailConflict<CustomerResponseDto>("Email is already registered.");

        customer.FullName = request.FullName.Trim();
        customer.Email = normalizedEmail;
        customer.Phone = StringNormalizer.NormalizeOptional(request.Phone);
        customer.Address = StringNormalizer.NormalizeOptional(request.Address);

        _customerRepository.Update(customer);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Customer updated successfully.", CustomerMapper.ToResponse(customer));
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

        return ApiResponseFactory.Ok("Customers searched successfully.", customers.Select(CustomerMapper.ToResponse).ToList());
    }

    public async Task<ApiResponse<List<SaleResponse>>> GetPurchasesAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
            return ApiResponseFactory.FailNotFound<List<SaleResponse>>("Customer not found.");

        var purchases = await _saleRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        return ApiResponseFactory.Ok("Customer purchases retrieved successfully.", purchases.Select(s => SaleMapper.ToSaleResponse(s)).ToList());
    }

    public async Task<ApiResponse<List<AppointmentResponse>>> GetServicesAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
            return ApiResponseFactory.FailNotFound<List<AppointmentResponse>>("Customer not found.");

        var services = await _appointmentRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        return ApiResponseFactory.Ok("Customer services retrieved successfully.", services.Select(a => AppointmentMapper.ToResponse(a)).ToList());
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
}