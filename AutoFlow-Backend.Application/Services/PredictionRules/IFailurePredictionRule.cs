using AutoFlow_Backend.Application.DTOs.Predictions;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Services.PredictionRules;

public interface IFailurePredictionRule
{
    bool Applies(Vehicle vehicle, int vehicleAge);
    IEnumerable<PredictedFailureResponse> GetFailures(Vehicle vehicle);
}
