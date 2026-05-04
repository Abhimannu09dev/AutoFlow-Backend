using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Reviews;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IAppDbContext _dbContext;

    public ReviewService(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<ReviewResponse>> CreateAsync(
        CreateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CustomerId == Guid.Empty)
            return Fail<ReviewResponse>("CustomerId is required.");

        var review = new Review
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Rating = request.Rating,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Reviews.AddAsync(review, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Success("Review created successfully.", Map(review));
    }

    public async Task<ApiResponse<List<ReviewResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var results = await _dbContext.Reviews
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => Map(r))
            .ToListAsync(cancellationToken);

        return Success("Reviews retrieved successfully.", results);
    }

    private static ReviewResponse Map(Review r) => new()
    {
        Id = r.Id,
        CustomerId = r.CustomerId,
        Rating = r.Rating,
        Comment = r.Comment,
        CreatedAt = r.CreatedAt
    };

    private static ApiResponse<T> Success<T>(string message, T data) =>
        new() { Status = true, Message = message, Data = data };

    private static ApiResponse<T> Fail<T>(string message) =>
        new() { Status = false, Message = message, Data = default };
}
