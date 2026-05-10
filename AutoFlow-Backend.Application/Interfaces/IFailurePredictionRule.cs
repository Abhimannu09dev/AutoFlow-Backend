using AutoFlow_Backend.Application.DTOs.Predictions;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IFailurePredictionRule
{
    bool Applies(Vehicle vehicle, int vehicleAge);
    IEnumerable<PredictedFailureResponse> GetFailures(Vehicle vehicle);
}