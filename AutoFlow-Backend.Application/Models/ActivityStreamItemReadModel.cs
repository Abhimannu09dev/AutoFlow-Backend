namespace AutoFlow_Backend.Application.Models;

public sealed record ActivityStreamItemReadModel(
    string Id,
    string Type,
    string Title,
    string Description,
    DateTime CreatedAt,
    string ActorName);
