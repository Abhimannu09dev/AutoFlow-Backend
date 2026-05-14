namespace AutoFlow_Backend.Application.Models;

public sealed record IdentityUserProfileReadModel(
    Guid UserId,
    string Email,
    string FullName,
    string? Phone,
    string? Address,
    IReadOnlyCollection<string> Roles);
