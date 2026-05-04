using System;
using System.Collections.Generic;
using System.Text;

namespace AutoFlow_Backend.Application.DTOs.Sales
{
    public class SaleItemRequest
    {
        public Guid PartId { get; set; }
        public int Quantity { get; set; }
        }
}
