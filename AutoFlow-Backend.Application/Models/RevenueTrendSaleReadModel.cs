namespace AutoFlow_Backend.Application.Models;

public sealed record RevenueTrendSaleReadModel(
    DateTime SaleDate,
    decimal TotalAmount);
