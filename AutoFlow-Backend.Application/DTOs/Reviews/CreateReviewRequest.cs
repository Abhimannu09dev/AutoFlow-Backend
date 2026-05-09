using System.ComponentModel.DataAnnotations;

namespace AutoFlow_Backend.Application.DTOs.Reviews;

public class CreateReviewRequest
{
    public Guid? CustomerId { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }
}
