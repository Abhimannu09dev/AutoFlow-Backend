using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Predictions;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/predictions")]
[Authorize]
[Tags("Predictions")]
public class PredictionsController : ControllerBase
{
    private readonly IFailurePredictionService _failurePredictionService;

    public PredictionsController(IFailurePredictionService failurePredictionService)
    {
        _failurePredictionService = failurePredictionService;
    }

    /// <summary>
    /// Get vehicle failure predictions for a customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of predicted maintenance needs based on vehicle mileage/age</returns>
    [HttpGet("{customerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<FailurePredictionResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<FailurePredictionResponse>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPredictions(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var result = await _failurePredictionService.GetPredictionsAsync(customerId, cancellationToken);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
}