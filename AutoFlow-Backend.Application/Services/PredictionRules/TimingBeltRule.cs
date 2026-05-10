using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Predictions;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Services.PredictionRules;

public class TimingBeltRule : IFailurePredictionRule
{
    public bool Applies(Vehicle vehicle, int vehicleAge) => vehicle.Mileage > FailurePredictionRules.TimingBeltMileageThreshold;

    public IEnumerable<PredictedFailureResponse> GetFailures(Vehicle vehicle) =>
    [
        new()
        {
            PartName = "Timing Belt",
            Reason = $"Vehicle has {vehicle.Mileage:N0} km. Timing belt replacement recommended after {FailurePredictionRules.TimingBeltMileageThreshold:N0} km.",
            Severity = "High"
        },
        new()
        {
            PartName = "Water Pump",
            Reason = $"Vehicle has {vehicle.Mileage:N0} km. Water pump typically fails around {FailurePredictionRules.TimingBeltMileageThreshold:N0}-{FailurePredictionRules.TransmissionFluidMileageThreshold:N0} km.",
            Severity = "High"
        },
        new()
        {
            PartName = "Spark Plugs",
            Reason = $"Vehicle has {vehicle.Mileage:N0} km. Spark plugs due for replacement.",
            Severity = "Medium"
        }
    ];
}
