using AutoFlow_Backend.Application.DTOs.Reviews;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Mappers;

public static class ReviewMapper
{
    public static ReviewResponse ToResponse(this Review review)
    {
        return new ReviewResponse
        {
            Id = review.Id,
            CustomerId = review.CustomerId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }
}