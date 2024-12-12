namespace Identity.API.Data.Dtos.Login;
public record TokenDto(
    string AccessToken,
    string RefreshToken
);