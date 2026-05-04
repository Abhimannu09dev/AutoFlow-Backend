namespace AutoFlow_Backend.Application.DTOs.Reviews;

public class CreateReviewRequest
{
    public Guid CustomerId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
