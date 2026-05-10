using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Appointments;
using AutoFlow_Backend.Application.DTOs.Customers;
using AutoFlow_Backend.Application.DTOs.Sales;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/customers/me")]
[Authorize(Roles = "Customer")]
[Tags("Customer Self-Service")]
public class CustomerHistoryController : BaseController
{
    private readonly ICustomerSelfService _customerSelfService;

    public CustomerHistoryController(ICustomerSelfService customerSelfService)
    {
        _customerSelfService = customerSelfService;
    }

    /// <summary>
    /// [Customer] Get your own purchase history
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Your purchase history</returns>
    [HttpGet("purchases")]
    [ProducesResponseType(typeof(ApiResponse<List<SaleResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<SaleResponse>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<SaleResponse>>>> GetMyPurchases(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponseFactory.Fail<List<SaleResponse>>("Invalid user token."));

        var response = await _customerSelfService.GetMyPurchasesAsync(userId.Value, cancellationToken);
        return response.ToActionResult();
    }

    /// <summary>
    /// [Customer] Get your own service history (appointments)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Your service history</returns>
    [HttpGet("services")]
    [ProducesResponseType(typeof(ApiResponse<List<AppointmentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<AppointmentResponse>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<AppointmentResponse>>>> GetMyServices(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponseFactory.Fail<List<AppointmentResponse>>("Invalid user token."));

        var response = await _customerSelfService.GetMyServicesAsync(userId.Value, cancellationToken);
        return response.ToActionResult();
    }

    /// <summary>
    /// [Customer] Get your own profile
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Your profile details</returns>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponseFactory.Fail<CustomerResponseDto>("Invalid user token."));

        var response = await _customerSelfService.GetMyProfileAsync(userId.Value, cancellationToken);
        return response.ToActionResult();
    }

    /// <summary>
    /// [Customer] Update your own profile
    /// </summary>
    /// <param name="request">Updated profile details (FullName, Phone, Address - Email cannot be changed)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated profile details</returns>
    [HttpPatch("profile")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> UpdateMyProfile(
        [FromBody] CustomerPatchDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponseFactory.Fail<CustomerResponseDto>("Invalid user token."));

        var response = await _customerSelfService.UpdateMyProfileAsync(userId.Value, request, cancellationToken);
        return response.ToActionResult();
    }
}