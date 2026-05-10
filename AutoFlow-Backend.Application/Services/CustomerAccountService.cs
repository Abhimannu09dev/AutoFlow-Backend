using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Models;
using Microsoft.Extensions.Logging;

namespace AutoFlow_Backend.Application.Services;

public class CustomerAccountService : ICustomerAccountService
{
    private const string CustomerRole = "Customer";

    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;
    private readonly ILogger<CustomerAccountService> _logger;

    public CustomerAccountService(
        IIdentityService identityService,
        IEmailService emailService,
        ILogger<CustomerAccountService> logger)
    {
        _identityService = identityService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ApiResponse<CustomerAccountResult>> ProvisionAsync(
        string fullName,
        string normalizedEmail,
        string? phone,
        string? address,
        CancellationToken cancellationToken = default)
    {
        var tempPassword = PasswordGenerator.Generate();

        var (userCreated, userId, createError) = await _identityService.CreateUserAsync(
            email: normalizedEmail,
            password: tempPassword,
            fullName: fullName,
            phone: phone,
            address: address,
            cancellationToken: cancellationToken);

        if (!userCreated || userId is null)
        {
            _logger.LogError("Failed to create user account for customer {Email}: {Error}", normalizedEmail, createError);
            return ApiResponseFactory.Fail<CustomerAccountResult>(createError ?? "Failed to create user account.");
        }

        var applicationUserId = Guid.Parse(userId);

        var roleExists = await _identityService.RoleExistsAsync(CustomerRole, cancellationToken);
        if (!roleExists)
        {
            await _identityService.DeleteUserAsync(userId, cancellationToken);
            return ApiResponseFactory.Fail<CustomerAccountResult>("Customer role is not configured.");
        }

        var (roleAssigned, roleError) = await _identityService.AssignRoleAsync(userId, CustomerRole, cancellationToken);
        if (!roleAssigned)
        {
            await _identityService.DeleteUserAsync(userId, cancellationToken);
            _logger.LogError("Failed to assign Customer role to user {UserId}: {Error}", userId, roleError);
            return ApiResponseFactory.Fail<CustomerAccountResult>(roleError ?? "Failed to assign customer role.");
        }

        var welcomeMessage = await SendWelcomeEmailAsync(fullName, normalizedEmail, tempPassword, cancellationToken);

        return ApiResponseFactory.Ok(welcomeMessage, new CustomerAccountResult(applicationUserId, welcomeMessage));
    }

    private async Task<string> SendWelcomeEmailAsync(
        string fullName,
        string normalizedEmail,
        string tempPassword,
        CancellationToken cancellationToken)
    {
        try
        {
            var emailBody = $@"
                <h2>Welcome to AutoFlow!</h2>
                <p>Hi {fullName},</p>
                <p>Your customer account has been created.</p>
                <p><strong>Login Email:</strong> {normalizedEmail}</p>
                <p><strong>Temporary Password:</strong> {tempPassword}</p>
                <p>Please log in and change your password after first login.</p>
                <p>Thank you,<br/>AutoFlow Team</p>
            ";

            await _emailService.SendAsync(
                normalizedEmail,
                "Your AutoFlow Customer Account",
                emailBody,
                cancellationToken);

            return "Customer account created successfully. Login details have been sent to the customer email.";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email to {Email}. Password is: {Password}", normalizedEmail, tempPassword);
            return "Customer account created successfully. Email sending failed - please notify customer of their temporary password.";
        }
    }
}