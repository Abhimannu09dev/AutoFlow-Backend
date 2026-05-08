using System.ComponentModel.DataAnnotations;

namespace AutoFlow_Backend.Application.DTOs.Customers;

public class CustomerCreateDto
{
    /// <summary>
    /// Full name of the customer
    /// </summary>
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Customer's email address (must be unique across both user accounts and customer records)
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Customer's phone number (optional)
    /// </summary>
    [MaxLength(30)]
    public string? Phone { get; set; }

    /// <summary>
    /// Customer's address (optional)
    /// </summary>
    [MaxLength(300)]
    public string? Address { get; set; }

    /// <summary>
    /// When true, creates a linked login account and sends temporary password via email
    /// </summary>
    public bool CreateLoginAccount { get; set; } = false;
}