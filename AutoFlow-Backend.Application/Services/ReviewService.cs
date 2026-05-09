using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Reviews;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ICustomerRepository _customerRepository;

    public ReviewService(
        IReviewRepository reviewRepository,
        ICustomerRepository customerRepository)
    {
        _reviewRepository = reviewRepository;
        _customerRepository = customerRepository;
    }

    public async Task<ApiResponse<ReviewResponse>> CreateAsync(
        CreateReviewRequest request,
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default)
    {
        if (request.Rating < 1 || request.Rating > 5)
            return ApiResponseFactory.Fail<ReviewResponse>("Rating must be between 1 and 5.");

        Guid customerId;

        if (isStaffOrAdmin && request.CustomerId.HasValue)
        {
            customerId = request.CustomerId.Value;
        }
        else if (requestingUserId.HasValue)
        {
            var customer = await _customerRepository.GetByApplicationUserIdAsync(requestingUserId.Value, cancellationToken);
            if (customer is null)
                return ApiResponseFactory.Fail<ReviewResponse>("Customer profile not found. Please contact support.");
            customerId = customer.Id;
        }
        else
        {
            return ApiResponseFactory.Fail<ReviewResponse>("Unable to determine customer.");
        }

        var review = new Review
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Rating = request.Rating,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _reviewRepository.AddAsync(review, cancellationToken);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Review created successfully.", Map(review));
    }

    public async Task<ApiResponse<List<ReviewResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
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