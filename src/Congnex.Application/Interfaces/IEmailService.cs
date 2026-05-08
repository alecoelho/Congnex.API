namespace Congnex.Application.Interfaces;

public interface IEmailService
{
    Task SendWelcomeAsync(string toEmail, string toName);
    Task SendPasswordResetAsync(string toEmail, string toName, string resetLink);
}
