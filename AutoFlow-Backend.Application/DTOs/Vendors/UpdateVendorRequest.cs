namespace AutoFlow_Backend.Application.DTOs.Vendors;

public class UpdateVendorRequest
{
    public string VendorName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
}
