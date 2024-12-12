namespace Identity.API.Data.Dtos.Login;

public record AccessInfoDto(
    string AccessToken, 
    string RefreshToken,
    DateTime TokenExpireTime);