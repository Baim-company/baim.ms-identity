namespace Identity.API.Data.Dtos.Email;

public record EmailResponseDto(
    string Status,
    string Message,
    string RedirectUrl
);