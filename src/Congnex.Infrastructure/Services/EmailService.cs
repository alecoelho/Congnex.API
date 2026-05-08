using Azure.Communication.Email;
using Congnex.Application.Interfaces;
using Congnex.Application.Settings;
using Microsoft.Extensions.Options;

namespace Congnex.Infrastructure.Services;

public sealed class EmailService(IOptions<AzureSettings> opts) : IEmailService
{
    private readonly CommunicationServicesSettings _cfg = opts.Value.CommunicationServices;

    private EmailClient Client() => new EmailClient(_cfg.ConnectionString);

    public async Task SendWelcomeAsync(string toEmail, string toName)
    {
        var message = new EmailMessage(
            senderAddress: _cfg.SenderEmail,
            recipients: new EmailRecipients([new EmailAddress(toEmail, toName)]),
            content: new EmailContent("Welcome to Congnex!")
            {
                PlainText = $"Hi {toName},\n\nWelcome to Congnex! Start your English learning journey today.",
                Html = $"<h2>Welcome to Congnex, {toName}!</h2><p>Start learning English today.</p>"
            });

        await Client().SendAsync(Azure.WaitUntil.Completed, message);
    }

    public async Task SendPasswordResetAsync(string toEmail, string toName, string resetLink)
    {
        var message = new EmailMessage(
            senderAddress: _cfg.SenderEmail,
            recipients: new EmailRecipients([new EmailAddress(toEmail, toName)]),
            content: new EmailContent("Congnex — Reset your password")
            {
                PlainText = $"Hi {toName},\n\nReset your password here: {resetLink}\n\nLink expires in 1 hour.",
                Html = $"<h2>Reset your password</h2><p><a href='{resetLink}'>Click here</a> to reset your password. Link expires in 1 hour.</p>"
            });

        await Client().SendAsync(Azure.WaitUntil.Completed, message);
    }
}
