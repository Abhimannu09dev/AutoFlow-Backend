namespace AutoFlow_Backend.Application.Models;

public record SaleSummaryReadModel(
    decimal SubTotal,
    decimal DiscountAmount,
    decimal TotalAmount);