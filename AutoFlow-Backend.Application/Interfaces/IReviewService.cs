using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Reviews;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IReviewService
{
    Task<ApiResponse<ReviewResponse>> CreateAsync(CreateReviewRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<ReviewResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
}
