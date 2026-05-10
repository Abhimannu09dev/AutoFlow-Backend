namespace AutoFlow_Backend.Application.Models;

public record OverdueCreditSaleReadModel(
    Guid SaleId,
    Guid CustomerId,
    DateTime SaleDate,
    decimal TotalAmount,
    string CustomerName,
    string CustomerEmail,
    string? Phone,
    string? Address);