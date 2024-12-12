namespace Identity.API.Data.Dtos.Login;

public record LoginDataDto(
    string email,
    string login,
    string password
);