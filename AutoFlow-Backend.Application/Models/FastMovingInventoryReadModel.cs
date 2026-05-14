namespace AutoFlow_Backend.Application.Models;

public sealed record FastMovingInventoryReadModel(
    Guid PartId,
    string PartName,
    string PartNumber,
    int SoldQuantity,
    int CurrentStock,
    decimal Revenue);
