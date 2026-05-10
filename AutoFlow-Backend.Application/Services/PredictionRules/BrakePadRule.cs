using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Predictions;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Services.PredictionRules;

public class BrakePadRule : IFailurePredictionRule
{
    public bool Applies(Vehicle vehicle, int vehicleAge) => vehicle.Mileage > FailurePredictionRules.BrakePadMileageThreshold;

    public IEnumerable<PredictedFailureResponse> GetFailures(Vehicle vehicle) =>
    [
        new()
        {
            PartName = "Brake Pads",
            Reason = $"Vehicle has {vehicle.Mileage:N0} km. Brake pads typically need replacement after {FailurePredictionRules.BrakePadMileageThreshold:N0} km.",
            Severity = "High"
        },
        new()
        {
            PartName = "Air Filter",
            Reason = $"Vehicle has {vehicle.Mileage:N0} km. Air filter replacement overdue.",
            Severity = "Low"
        },
        new()
        {
            PartName = "Oil Filter",
            Reason = $"Vehicle has {vehicle.Mileage:N0} km. Oil filter should be replaced regularly.",
            Severity = "Low"
        }
    ];
}
