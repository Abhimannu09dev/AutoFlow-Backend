namespace AutoFlow_Backend.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}