namespace Identity.API.Data.Dtos.Email;

public record EmailConfigurationDto(
    string From,
    string SmtpServer,
    int Port,
    string UserName,
    string Password
);