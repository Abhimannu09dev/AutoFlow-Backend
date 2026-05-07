using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/predictions")]
[Authorize]
public class PredictionsController : ControllerBase
{
    private readonly IFailurePredictionService _failurePredictionService;

    public PredictionsController(IFailurePredictionService failurePredictionService)
    {
        _failurePredictionService = failurePredictionService;
    }

    [HttpGet("{customerId:guid}")]
    public async Task<IActionResult> GetPredictions(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var result = await _failurePredictionService.GetPredictionsAsync(customerId, cancellationToken);
        return result.Status ? Ok(result) : NotFound(result);
    }
}