
using AutoFlow_Backend.Domain.Enums;

namespace AutoFlow_Backend.Application.DTOs.Sales
{
    public class CreateSaleRequest
    {
        public Guid CustomerId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? Notes { get; set; }
        public List<SaleItemRequest> Items { get; set; } = new List<SaleItemRequest>();
    }
}
