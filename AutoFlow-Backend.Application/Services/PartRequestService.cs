using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.PartRequests;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Domain.Enums;

namespace AutoFlow_Backend.Application.Services;

public class PartRequestService : IPartRequestService
{
    private readonly IPartRequestRepository _partRequestRepository;

    public PartRequestService(IPartRequestRepository partRequestRepository)
    {
        _partRequestRepository = partRequestRepository;
    }

    public async Task<ApiResponse<PartRequestResponse>> CreateAsync(
        CreatePartRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CustomerId == Guid.Empty)
            return ApiResponseFactory.Fail<PartRequestResponse>("CustomerId is required.");

        if (string.IsNullOrWhiteSpace(request.PartName))
            return ApiResponseFactory.Fail<PartRequestResponse>("PartName is required.");

        var status = Enum.TryParse<PartRequestStatus>(request.Status, ignoreCase: true, out var parsed)
            ? parsed
            : PartRequestStatus.Pending;

        var partRequest = new PartRequest
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            PartName = request.PartName.Trim(),
            Quantity = request.Quantity,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

        await _partRequestRepository.AddAsync(partRequest, cancellationToken);
        await _partRequestRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Part request created successfully.", Map(partRequest));
    }

    public async Task<ApiResponse<List<PartRequestResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var results = await _partRequestRepository.GetAllAsync(cancellationToken);
        return ApiResponseFactory.Ok("Part requests retrieved successfully.", results.Select(Map).ToList());
    }

    private static PartRequestResponse Map(PartRequest pr) => new()
    {
        Id = pr.Id,
        CustomerId = pr.CustomerId,
        PartName = pr.PartName,
        Quantity = pr.Quantity,
        Status = pr.Status.ToString(),
        CreatedAt = pr.CreatedAt,
        UpdatedAt = pr.UpdatedAt
    };
}