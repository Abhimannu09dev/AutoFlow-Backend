namespace AutoFlow_Backend.Application.DTOs.Predictions;

public class FailurePredictionResponse
{
    public Guid VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Mileage { get; set; }
    public List<PredictedFailureResponse> PredictedFailures { get; set; } = new();
}