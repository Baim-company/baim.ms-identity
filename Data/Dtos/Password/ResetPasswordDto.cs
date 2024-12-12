namespace Identity.API.Data.Dtos.Password;

public record ResetPasswordDto(
    string Password,
    string ConfirmPassword,
    string Token
);