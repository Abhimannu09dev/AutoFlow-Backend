using AutoFlow_Backend.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace AutoFlow_Backend.Infrastructure.Configuration;

public class NotificationSettings : INotificationSettings
{
    private readonly EmailSettings _emailSettings;

    public NotificationSettings(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public string AdminEmail => _emailSettings.AdminEmail;
}