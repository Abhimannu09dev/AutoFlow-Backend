using AutoFlow_Backend.Domain.Enums;

namespace AutoFlow_Backend.Domain.Entities;

public class Sale
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid StaffId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public SaleStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime? InvoiceSentAt { get; set; }
    public string? InvoiceEmail { get; set; }
    public DateTime? InvoiceFailedAt { get; set; }
    public string? InvoiceFailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Customer? Customer { get; set; }
    public Staff? Staff { get; set; }
    public CreditStatus? CreditStatus { get; set; }
    public DateTime? DueDate { get; set; }
    public ICollection<CreditPayment> CreditPayments { get; set; } = [];
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}