namespace Identity.API.Services.Abstractions;

public interface IPasswordService
{
    string GenerateRandomPassword();
}
