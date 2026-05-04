using AutoFlow_Backend.Domain.Enums;
using System.Net.ServerSentEvents;

namespace AutoFlow_Backend.Domain.Entities;

public class Sale
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid StaffId { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public SaleStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Customer? Customer { get; set; }
    public ICollection<SaleItems> SaleItems { get; set; } = new List<SaleItems>();
}