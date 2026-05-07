using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.PartRequests;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Application.Services;

public class PartRequestService : IPartRequestService
{
    private readonly IAppDbContext _dbContext;

    public PartRequestService(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<PartRequestResponse>> CreateAsync(
        CreatePartRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CustomerId == Guid.Empty)
            return ApiResponseFactory.Fail<PartRequestResponse>("CustomerId is required.");

        if (string.IsNullOrWhiteSpace(request.PartName))
            return ApiResponseFactory.Fail<PartRequestResponse>("PartName is required.");

        var partRequest = new PartRequest
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            PartName = request.PartName.Trim(),
            Quantity = request.Quantity,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Pending" : request.Status.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.PartRequests.AddAsync(partRequest, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Part request created successfully.", Map(partRequest));
    }

    public async Task<ApiResponse<List<PartRequestResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var results = await _dbContext.PartRequests
            .AsNoTracking()
            .OrderByDescending(pr => pr.CreatedAt)
            .Select(pr => Map(pr))
            .ToListAsync(cancellationToken);

        return ApiResponseFactory.Ok("Part requests retrieved successfully.", results);
    }

    private static PartRequestResponse Map(PartRequest pr) => new()
    {
        Id = pr.Id,
        CustomerId = pr.CustomerId,
        PartName = pr.PartName,
        Quantity = pr.Quantity,
        Status = pr.Status,
        CreatedAt = pr.CreatedAt,
        UpdatedAt = pr.UpdatedAt
    };
}
