using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Predictions;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Services.PredictionRules;

public class CoolantRule : IFailurePredictionRule
{
    public bool Applies(Vehicle vehicle, int vehicleAge) => vehicleAge > FailurePredictionRules.CoolantAgeThresholdYears;

    public IEnumerable<PredictedFailureResponse> GetFailures(Vehicle vehicle) =>
    [
        new()
        {
            PartName = "Coolant",
            Reason = $"Vehicle is {DateTime.UtcNow.Year - vehicle.Year} years old. Coolant flush recommended every {FailurePredictionRules.CoolantAgeThresholdYears} years.",
            Severity = "Medium"
        }
    ];
}
