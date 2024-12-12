using Identity.API.Data.Dtos.Email;

namespace Identity.API.Services.Abstractions;

public interface IEmailService
{
    void SendEmail(EmailMessageDto message, string confirmLink, string? password, string? email);
    void SendEmail(EmailMessageDto message, string? password, string? login);
}