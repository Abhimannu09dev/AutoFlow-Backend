using System;
using System.Collections.Generic;
using System.Text;

namespace AutoFlow_Backend.Application.DTOs.Sales
{
    public class SaleItemResponse
    {
        public Guid Id { get; set; }
        public Guid PartId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
    }
}
