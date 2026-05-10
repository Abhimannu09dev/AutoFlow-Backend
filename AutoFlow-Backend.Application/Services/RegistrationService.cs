using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Auth;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AutoFlow_Backend.Application.Services;

public class RegistrationService : IRegistrationService
{
    private const string CustomerRole = "Customer";

    private readonly IIdentityService _identityService;
    private readonly ICustomerRepository _customerRepository;
    private readonly IAuthService _authService;
    private readonly ILogger<RegistrationService> _logger;

    public RegistrationService(
        IIdentityService identityService,
        ICustomerRepository customerRepository,
        IAuthService authService,
        ILogger<RegistrationService> logger)
    {
        _identityService = identityService;
        _customerRepository = customerRepository;
        _authService = authService;
        _logger = logger;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var emailNormalized = request.Email.ToUpperInvariant();
        var userExists = await _identityService.UserExistsByEmailAsync(emailNormalized, cancellationToken: cancellationToken);
        if (userExists)
            return ApiResponseFactory.FailConflict<AuthResponse>("Email is already registered.");

        var (userCreated, userId, createError) = await _identityService.CreateUserAsync(
            email: request.Email,
            password: request.Password,
            fullName: request.FullName,
            phone: request.Phone,
            address: request.Address,
            cancellationToken: cancellationToken);

        if (!userCreated || userId is null)
        {
            _logger.LogError("Failed to create user for registration {Email}: {Error}", request.Email, createError);
            return ApiResponseFactory.Fail<AuthResponse>(createError ?? "Failed to create user account.");
        }

        var applicationUserId = Guid.Parse(userId);

        var roleExists = await _identityService.RoleExistsAsync(CustomerRole, cancellationToken);
        if (!roleExists)
        {
            await _identityService.DeleteUserAsync(userId, cancellationToken);
            return ApiResponseFactory.Fail<AuthResponse>("Customer role is not configured.");
        }

        var (roleAssigned, roleError) = await _identityService.AssignRoleAsync(userId, CustomerRole, cancellationToken);
        if (!roleAssigned)
        {
            await _identityService.DeleteUserAsync(userId, cancellationToken);
            _logger.LogError("Failed to assign Customer role to user {UserId}: {Error}", userId, roleError);
            return ApiResponseFactory.Fail<AuthResponse>(roleError ?? "Failed to assign customer role.");
        }

        var customer = new Customer
        {
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            CreatedAt = DateTime.UtcNow,
            ApplicationUserId = applicationUserId
        };

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer registered successfully: {Email}", request.Email);

        return await _authService.LoginAsync(
            new LoginRequest { Email = request.Email, Password = request.Password },
            cancellationToken);
    }
}