using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace ChurchAdmin.Api.Common;

public sealed class EmailService
{
    private readonly EmailSettings _settings;
    private readonly FrontendSettings _frontendSettings;

    public EmailService(
        IOptions<EmailSettings> settings,
        IOptions<FrontendSettings> frontendSettings)
    {
        _settings = settings.Value;
        _frontendSettings = frontendSettings.Value;
    }

    public async Task SendInviteEmailAsync(
        string toEmail,
        string displayName,
        string inviteLink)
    {
        ValidateSettings();

        string productName = string.IsNullOrWhiteSpace(_frontendSettings.ProductName)
            ? "Church Admin"
            : _frontendSettings.ProductName;

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = $"You're invited to {productName}",
            Body = $@"
Hello {displayName},

You have been invited to {productName}.

Click the link below to set your password:

{inviteLink}

This link expires in 7 days.

Regards,
{productName}
",
            IsBodyHtml = false
        };

        message.To.Add(toEmail);

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        await client.SendMailAsync(message);
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.Host) ||
            string.IsNullOrWhiteSpace(_settings.Username) ||
            string.IsNullOrWhiteSpace(_settings.Password) ||
            string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            throw new InvalidOperationException("EmailSettings are incomplete. Configure SMTP settings before sending invites.");
        }
    }
}
