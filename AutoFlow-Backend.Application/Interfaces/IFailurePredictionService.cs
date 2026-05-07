using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Predictions;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IFailurePredictionService
{
    Task<ApiResponse<List<FailurePredictionResponse>>> GetPredictionsAsync(Guid customerId, CancellationToken cancellationToken = default);
}