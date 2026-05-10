using AutoFlow_Backend.Application.DTOs.PartRequests;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Mappers;

public static class PartRequestMapper
{
    public static PartRequestResponse ToResponse(this PartRequest partRequest)
    {
        return new PartRequestResponse
        {
            Id = partRequest.Id,
            CustomerId = partRequest.CustomerId,
            PartName = partRequest.PartName,
            Quantity = partRequest.Quantity,
            Status = partRequest.Status,
            CreatedAt = partRequest.CreatedAt,
            UpdatedAt = partRequest.UpdatedAt
        };
    }
}