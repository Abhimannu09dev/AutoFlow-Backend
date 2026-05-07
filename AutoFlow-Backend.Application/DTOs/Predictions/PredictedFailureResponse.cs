namespace AutoFlow_Backend.Application.DTOs.Predictions;

public class PredictedFailureResponse
{
    public string PartName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
}