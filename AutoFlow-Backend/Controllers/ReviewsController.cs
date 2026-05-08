using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Reviews;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/reviews")]
[Authorize(Roles = "Customer,Admin,Staff")]
[Tags("Reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// [Customer, Staff, Admin] Create a new customer review. Customers can review their own service experience.
    /// </summary>
    /// <param name="request">Review details (CustomerId, Rating 1-5, Comment)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created review details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ReviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ReviewResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> Create(
        [FromBody] CreateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _reviewService.CreateAsync(request, cancellationToken);
        if (!response.IsSuccess)
            return BadRequest(response);
        return Ok(response);
    }

    /// <summary>
    /// [Customer, Staff, Admin] Get all customer reviews. Staff/Admin see all reviews.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all customer reviews</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ReviewResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ReviewResponse>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var response = await _reviewService.GetAllAsync(cancellationToken);
        return Ok(response);
    }
}