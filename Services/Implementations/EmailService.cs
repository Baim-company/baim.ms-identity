using Identity.API.Data.Dtos.Email;
using Identity.API.Services.Abstractions;
using MailKit.Net.Smtp;
using MimeKit;
using System.Text.RegularExpressions;

namespace Identity.API.Services.Implementations;


public class EmailService : IEmailService
{
    private readonly EmailConfigurationDto _emailConfig;

    public EmailService(EmailConfigurationDto emailConfig) => _emailConfig = emailConfig;


    public void SendEmail(EmailMessageDto message, string confirmLink, string? password, string? email)
    {
        var emailMessage = CreateEmailMessage(message, confirmLink, password, email);
        Send(emailMessage);
    }

    public void SendEmail(EmailMessageDto message, string? password, string? login)
    {
        var emailMessage = CreateEmailMessage(message, password, login);
        Send(emailMessage);
    }

    private MimeMessage CreateEmailMessage(EmailMessageDto message, string confirmLink, string? password, string? email)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("Baim", _emailConfig.From));
        emailMessage.To.AddRange(message.To);
        emailMessage.Subject = message.Subject;

        string htmlContent = File.ReadAllText(message.HtmlFilePath);
        htmlContent = htmlContent.Replace("{{Link}}", confirmLink);
        if (email != null && password != null)
        {
            htmlContent = Regex.Replace(htmlContent, "{{Password}}", password, RegexOptions.IgnoreCase);
            htmlContent = Regex.Replace(htmlContent, "{{Email}}", email, RegexOptions.IgnoreCase);
        }

        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = htmlContent;

        emailMessage.Body = bodyBuilder.ToMessageBody();

        return emailMessage;
    }
    private MimeMessage CreateEmailMessage(EmailMessageDto message, string? password, string? login)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("Baim", _emailConfig.From));
        emailMessage.To.AddRange(message.To);
        emailMessage.Subject = message.Subject;

        string htmlContent = File.ReadAllText(message.HtmlFilePath);
        if (login != null && password != null)
        {
            htmlContent = Regex.Replace(htmlContent, "{{Password}}", password, RegexOptions.IgnoreCase);
            htmlContent = Regex.Replace(htmlContent, "{{Email}}", login, RegexOptions.IgnoreCase);
        }

        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = htmlContent;

        emailMessage.Body = bodyBuilder.ToMessageBody();

        return emailMessage;
    }

    private void Send(MimeMessage mailMessage)
    {
        using var client = new SmtpClient();
        try
        {
            client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, true);
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            client.Authenticate(_emailConfig.UserName, _emailConfig.Password);

            client.Send(mailMessage);
        }
        catch
        {
            throw;
        }
        finally
        {
            client.Disconnect(true);
            client.Dispose();
        }
    }
}