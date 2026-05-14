using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.PartRequests;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Application.Mappers;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Domain.Enums;

namespace AutoFlow_Backend.Application.Services;

public class PartRequestService : IPartRequestService
{
    private readonly IPartRequestRepository _partRequestRepository;
    private readonly ICustomerRepository _customerRepository;

    public PartRequestService(
        IPartRequestRepository partRequestRepository,
        ICustomerRepository customerRepository)
    {
        _partRequestRepository = partRequestRepository;
        _customerRepository = customerRepository;
    }

    public async Task<ApiResponse<PartRequestResponse>> CreateAsync(
        CreatePartRequestRequest request,
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.PartName))
            return ApiResponseFactory.Fail<PartRequestResponse>("PartName is required.");

        Guid customerId;

        if (isStaffOrAdmin && request.CustomerId.HasValue)
        {
            customerId = request.CustomerId.Value;
        }
        else if (requestingUserId.HasValue)
        {
            var customer = await _customerRepository.GetByApplicationUserIdAsync(requestingUserId.Value, cancellationToken);
            if (customer is null)
                return ApiResponseFactory.Fail<PartRequestResponse>("Customer profile not found. Please contact support.");
            customerId = customer.Id;
        }
        else
        {
            return ApiResponseFactory.Fail<PartRequestResponse>("Unable to determine customer.");
        }

        var status = Enum.TryParse<PartRequestStatus>(request.Status, ignoreCase: true, out var parsed)
            ? parsed
            : PartRequestStatus.Pending;

        var partRequest = new PartRequest
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            PartName = request.PartName.Trim(),
            Quantity = request.Quantity,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

        await _partRequestRepository.AddAsync(partRequest, cancellationToken);
        await _partRequestRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Part request created successfully.", partRequest.ToResponse());
    }

    public async Task<ApiResponse<PagedResponse<PartRequestResponse>>> GetAllAsync(
        PagedRequest request,
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default)
    {
        PagedResponse<PartRequest> paged;

        if (isStaffOrAdmin)
        {
            paged = await _partRequestRepository.GetPagedAsync(request, cancellationToken);
        }
        else if (requestingUserId.HasValue)
        {
            var customer = await _customerRepository.GetByApplicationUserIdAsync(requestingUserId.Value, cancellationToken);
            if (customer is null)
                return ApiResponseFactory.Fail<PagedResponse<PartRequestResponse>>("Customer profile not found.");

            paged = await _partRequestRepository.GetPagedByCustomerIdAsync(customer.Id, request, cancellationToken);
        }
        else
        {
            return ApiResponseFactory.Fail<PagedResponse<PartRequestResponse>>("Unable to determine user.");
        }

        return ApiResponseFactory.Ok("Part requests retrieved successfully.", paged.Map(p => p.ToResponse()));
    }

    public async Task<ApiResponse<PartRequestResponse>> UpdateStatusAsync(
        Guid id,
        UpdatePartRequestStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var partRequest = await _partRequestRepository.GetByIdAsync(id, cancellationToken);
        if (partRequest is null)
            return ApiResponseFactory.FailNotFound<PartRequestResponse>("Part request not found.");

        if (!TryMapStatus(request.Status, out var mappedStatus))
            return ApiResponseFactory.Fail<PartRequestResponse>(
                "Status must be one of: Pending, Done, Rejected.");

        partRequest.Status = mappedStatus;
        partRequest.UpdatedAt = DateTime.UtcNow;

        _partRequestRepository.Update(partRequest);
        await _partRequestRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Part request status updated successfully.", partRequest.ToResponse());
    }

    private static bool TryMapStatus(string? status, out PartRequestStatus mappedStatus)
    {
        mappedStatus = PartRequestStatus.Pending;

        if (string.IsNullOrWhiteSpace(status))
            return false;

        switch (status.Trim().ToLowerInvariant())
        {
            case "pending":
                mappedStatus = PartRequestStatus.Pending;
                return true;
            case "done":
            case "fulfilled":
                mappedStatus = PartRequestStatus.Fulfilled;
                return true;
            case "rejected":
                mappedStatus = PartRequestStatus.Rejected;
                return true;
            default:
                return false;
        }
    }
}
