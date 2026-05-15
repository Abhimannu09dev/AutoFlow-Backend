using AutoFlow_Backend.Domain.Enums;

namespace AutoFlow_Backend.Application.DTOs.Credits;

public class UpdateCreditStatusRequest
{
    public CreditStatus Status { get; set; }
}
