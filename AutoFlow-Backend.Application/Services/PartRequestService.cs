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

    public async Task<ApiResponse<List<PartRequestResponse>>> GetAllAsync(
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default)
    {
        List<PartRequest> results;

        if (isStaffOrAdmin)
        {
            results = await _partRequestRepository.GetAllAsync(cancellationToken);
        }
        else if (requestingUserId.HasValue)
        {
            var customer = await _customerRepository.GetByApplicationUserIdAsync(requestingUserId.Value, cancellationToken);
            if (customer is null)
                return ApiResponseFactory.Fail<List<PartRequestResponse>>("Customer profile not found.");

            results = await _partRequestRepository.GetByCustomerIdAsync(customer.Id, cancellationToken);
        }
        else
        {
            return ApiResponseFactory.Fail<List<PartRequestResponse>>("Unable to determine user.");
        }

        return ApiResponseFactory.Ok("Part requests retrieved successfully.", results.Select(p => p.ToResponse()).ToList());
    }
}