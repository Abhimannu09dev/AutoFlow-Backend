using AutoFlow_Backend.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoFlow_Backend.Application.DTOs.Sales
{
    public class SaleResponse
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public Guid StaffId { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool LoyaltyDiscountApplied { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public SaleStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime? InvoiceSentAt { get; set; }
        public string? InvoiceEmail { get; set; }
        public DateTime? InvoiceFailedAt { get; set; }
        public string? InvoiceFailureReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<SaleItemResponse> Items { get; set; } = new();
    }
}
