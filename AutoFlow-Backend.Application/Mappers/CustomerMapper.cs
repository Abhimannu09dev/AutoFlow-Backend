using AutoFlow_Backend.Application.DTOs.Customers;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Mappers;

public static class CustomerMapper
{
    public static CustomerResponseDto ToResponse(Customer customer)
    {
        return new CustomerResponseDto
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            CreatedAt = customer.CreatedAt,
            ApplicationUserId = customer.ApplicationUserId
        };
    }
}