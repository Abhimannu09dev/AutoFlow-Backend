using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Reviews;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IReviewService
{
    Task<ApiResponse<ReviewResponse>> CreateAsync(
        CreateReviewRequest request,
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PagedResponse<ReviewResponse>>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default);
}
