using FlatScannerWeb.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FlatScannerWeb.Services;

public class MailService : IMailService
{
    private readonly MailSmtpOptions _mailSmtpOptions;

    public MailService(IOptions<MailSmtpOptions> mailSmtpOptions)
    {
        _mailSmtpOptions = mailSmtpOptions.Value;
    }

    public async Task SendEmail(IEnumerable<string> recipients, string subject, string messageBody)
    {
        if (!_mailSmtpOptions.Enabled)
            return;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_mailSmtpOptions.Name, _mailSmtpOptions.Mail));

        foreach (var recipient in recipients)
        {
            message.To.Add(new MailboxAddress("", recipient));
        }

        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = messageBody
        };
        message.Body = bodyBuilder.ToMessageBody();

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_mailSmtpOptions.Mail, _mailSmtpOptions.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
