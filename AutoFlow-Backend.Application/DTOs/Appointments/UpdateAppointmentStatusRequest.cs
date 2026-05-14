using AutoFlow_Backend.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace AutoFlow_Backend.Application.DTOs.Appointments;

public class UpdateAppointmentStatusRequest
{
    [Required]
    public AppointmentStatus Status { get; set; }
}
