namespace AutoFlow_Backend.Application.Models;

public record LowStockPartReadModel(
    Guid Id,
    string PartName,
    string PartNumber,
    int StockQuantity,
    int MinimumStockLevel);