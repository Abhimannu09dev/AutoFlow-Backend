using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Appointments;
using AutoFlow_Backend.Application.DTOs.Customers;
using AutoFlow_Backend.Application.DTOs.Sales;
using AutoFlow_Backend.Application.DTOs.Vehicles;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;

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

    private readonly ICustomerRepository _customerRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public CustomerService(
        ICustomerRepository customerRepository,
        IVehicleRepository vehicleRepository,
        ISaleRepository saleRepository,
        IAppointmentRepository appointmentRepository)
    {
        _customerRepository = customerRepository;
        _vehicleRepository = vehicleRepository;
        _saleRepository = saleRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<ApiResponse<CustomerResponseDto>> CreateAsync(
        CustomerCreateDto request,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = Validate(request.FullName, request.Email, request.Phone, request.Address);
        if (validationErrors.Count > 0)
            return ApiResponseFactory.FailFromValidation<CustomerResponseDto>(validationErrors);

        var normalizedEmail = NormalizeEmail(request.Email);
        var emailExists = await _customerRepository.EmailExistsAsync(normalizedEmail, null, cancellationToken);
        if (emailExists)
            return ApiResponseFactory.FailConflict<CustomerResponseDto>("Email is already registered.");

        var customer = new Customer
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            Phone = NormalizeOptional(request.Phone),
            Address = NormalizeOptional(request.Address),
            CreatedAt = DateTime.UtcNow
        };

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Customer created successfully.", Map(customer));
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

        var matchingUserIds = await _vehicleRepository.GetUserIdsByVehicleQueryAsync(normalizedLower, cancellationToken);
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
        var validationErrors = ValidateVehicle(request.VehicleNumber, request.Brand, request.Model, request.Year, request.Color, request.VIN);
        if (validationErrors.Count > 0)
            return ApiResponseFactory.FailFromValidation<VehicleResponseDto>(validationErrors);

        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
            return ApiResponseFactory.FailNotFound<VehicleResponseDto>("Customer not found.");

        if (customer.ApplicationUserId is null)
            return ApiResponseFactory.Fail<VehicleResponseDto>("Customer is not linked to a user account.");

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

        await _vehicleRepository.AddAsync(vehicle, cancellationToken);
        await _vehicleRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Vehicle added successfully.", MapVehicle(vehicle));
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

        var vehicles = await _vehicleRepository.GetByUserIdAsync(customer.ApplicationUserId.Value, cancellationToken);
        return ApiResponseFactory.Ok("Customer vehicles retrieved successfully.", vehicles.Select(MapVehicle).ToList());
    }

    private static CustomerResponseDto Map(Customer customer) => new()
    {
        Id = customer.Id,
        FullName = customer.FullName,
        Email = customer.Email,
        Phone = customer.Phone,
        Address = customer.Address,
        CreatedAt = customer.CreatedAt
    };

    private static VehicleResponseDto MapVehicle(Vehicle vehicle) => new()
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

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}