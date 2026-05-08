namespace AutoFlow_Backend.Application.DTOs.Customers;

public class CustomerResponseDto
{
    /// <summary>
    /// Unique identifier for the customer
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Full name of the customer
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Customer's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Customer's phone number
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Customer's address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Date and time when the customer was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Linked ApplicationUser ID (null if no login account created)
    /// </summary>
    public Guid? ApplicationUserId { get; set; }
}