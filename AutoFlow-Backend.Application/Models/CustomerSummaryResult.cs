namespace AutoFlow_Backend.Application.Models;

public sealed record CustomerSummaryResult(
    Guid CustomerId,
    string FullName,
    string Email,
    string? Phone,
    string? Address,
    int PurchaseCount,
    decimal TotalSpent,
    DateTime LastPurchaseDate);