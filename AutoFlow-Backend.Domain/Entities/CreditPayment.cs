using AutoFlow_Backend.Domain.Enums;

namespace AutoFlow_Backend.Domain.Entities;

public class CreditPayment
{
    public Guid Id { get; set; }
    public Guid SaleId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }

    public Sale? Sale { get; set; }
}
