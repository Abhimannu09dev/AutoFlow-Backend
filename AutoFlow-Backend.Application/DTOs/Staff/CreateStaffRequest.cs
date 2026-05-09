namespace AutoFlow_Backend.Application.DTOs.Staff;

public class CreateStaffRequest
{
    public string? StaffCode { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Role { get; set; }
}
