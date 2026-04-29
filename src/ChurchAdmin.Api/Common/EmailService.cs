using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace ChurchAdmin.Api.Common;

public sealed class EmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendInviteEmailAsync(
        string toEmail,
        string displayName,
        string inviteLink)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = "You're invited to La Borne Church Admin Portal",
            Body = $@"
Hello {displayName},

You have been invited to the La Borne Church Cape Durbanville Admin Portal.

Click the link below to set your password:

{inviteLink}

This link expires in 7 days.

Regards,
La Borne Church Cape Durbanville
",
            IsBodyHtml = false
        };

        message.To.Add(toEmail);

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(
                _settings.Username,
                _settings.Password)
        };

        await client.SendMailAsync(message);
    }
}