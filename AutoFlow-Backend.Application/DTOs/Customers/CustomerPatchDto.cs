using System.ComponentModel.DataAnnotations;

namespace AutoFlow_Backend.Application.DTOs.Customers;

public class CustomerPatchDto
{
    [MaxLength(150)]
    public string? FullName { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }
}