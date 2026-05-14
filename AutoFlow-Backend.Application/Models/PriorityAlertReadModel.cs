namespace AutoFlow_Backend.Application.Models;

public sealed record PriorityAlertReadModel(
    string Id,
    string Type,
    string Severity,
    string Title,
    string Description,
    DateTime CreatedAt);
