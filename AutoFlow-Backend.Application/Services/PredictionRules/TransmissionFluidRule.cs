using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Predictions;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Services.PredictionRules;

public class TransmissionFluidRule : IFailurePredictionRule
{
    public bool Applies(Vehicle vehicle, int vehicleAge) => vehicle.Mileage > FailurePredictionRules.TransmissionFluidMileageThreshold;

    public IEnumerable<PredictedFailureResponse> GetFailures(Vehicle vehicle) =>
    [
        new()
        {
            PartName = "Transmission Fluid",
            Reason = $"Vehicle has {vehicle.Mileage:N0} km. Transmission fluid change critical after {FailurePredictionRules.TransmissionFluidMileageThreshold:N0} km.",
            Severity = "High"
        },
        new()
        {
            PartName = "Shock Absorbers",
            Reason = $"Vehicle has {vehicle.Mileage:N0} km. Shock absorbers likely worn at this mileage.",
            Severity = "Medium"
        }
    ];
}
