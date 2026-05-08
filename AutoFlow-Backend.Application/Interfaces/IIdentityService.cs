namespace AutoFlow_Backend.Application.Interfaces;

public interface IIdentityService
{
    Task<(bool Succeeded, string? UserId, string? Error)> CreateUserAsync(
        string email,
        string password,
        string fullName,
        string? phone,
        string? address,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error)> AssignRoleAsync(
        string userId,
        string role,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error)> UpdateUserAsync(
        string userId,
        string email,
        string fullName,
        string? phone,
        string? address,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error)> LockUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<bool> UserExistsByEmailAsync(
        string normalizedEmail,
        string? excludeUserId = null,
        CancellationToken cancellationToken = default);

    Task<bool> RoleExistsAsync(
        string role,
        CancellationToken cancellationToken = default);

    Task DeleteUserAsync(
        string userId,
        CancellationToken cancellationToken = default);
}