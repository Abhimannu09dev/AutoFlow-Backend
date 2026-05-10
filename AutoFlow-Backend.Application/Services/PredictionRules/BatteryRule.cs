using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Predictions;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Services.PredictionRules;

public class BatteryRule : IFailurePredictionRule
{
    public bool Applies(Vehicle vehicle, int vehicleAge) => vehicleAge > FailurePredictionRules.BatteryAgeThresholdYears;

    public IEnumerable<PredictedFailureResponse> GetFailures(Vehicle vehicle) =>
    [
        new()
        {
            PartName = "Battery",
            Reason = $"Vehicle is {DateTime.UtcNow.Year - vehicle.Year} years old. Batteries typically last 3-5 years.",
            Severity = "High"
        },
        new()
        {
            PartName = "Serpentine Belt",
            Reason = $"Vehicle is {DateTime.UtcNow.Year - vehicle.Year} years old. Rubber belts degrade significantly after {FailurePredictionRules.BatteryAgeThresholdYears} years.",
            Severity = "Medium"
        }
    ];
}
