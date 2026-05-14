using AutoFlow_Backend.Application.Common;

namespace AutoFlow_Backend.Application.DTOs.Customers;

public class CustomerPagedRequest : PagedRequest
{
    public string? SearchTerm { get; set; }
}
