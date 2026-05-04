using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Reviews;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> Create(
        [FromBody] CreateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _reviewService.CreateAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ReviewResponse>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var response = await _reviewService.GetAllAsync(cancellationToken);
        return Ok(response);
    }
}
