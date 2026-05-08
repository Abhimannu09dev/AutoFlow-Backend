using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Reviews;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<ApiResponse<ReviewResponse>> CreateAsync(
        CreateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CustomerId == Guid.Empty)
            return ApiResponseFactory.Fail<ReviewResponse>("CustomerId is required.");

        var review = new Review
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Rating = request.Rating,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _reviewRepository.AddAsync(review, cancellationToken);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Review created successfully.", Map(review));
    }

    public async Task<ApiResponse<List<ReviewResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var results = await _reviewRepository.GetAllAsync(cancellationToken);
        return ApiResponseFactory.Ok("Reviews retrieved successfully.", results.Select(Map).ToList());
    }

    private static ReviewResponse Map(Review r) => new()
    {
        Id = r.Id,
        CustomerId = r.CustomerId,
        Rating = r.Rating,
        Comment = r.Comment,
        CreatedAt = r.CreatedAt
    };
}