namespace Identity.API.Data.Dtos.Password;

public record ChangePasswordDto(
    string Email,
    string OldPassword,
    string NewPassword
);