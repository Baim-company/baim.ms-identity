using Identity.API.Data.Dtos.Login;
using Identity.API.Data.Dtos.Password;
using Identity.API.Data.Dtos.Register;
using Identity.API.Data.Dtos.Responses;
using Identity.API.Data.Entities;

namespace Identity.API.Services.Abstractions;

public interface IAccountService
{
    Task<ResponseDto<ApplicationUser>> RegisterAsync(RegisterUserDto model, bool isOldClient);

    Task<bool> ConfirmEmailAsync(string token);

    Task<ResponseDto<ApplicationUser>> ChangePasswordAsync(ChangePasswordDto model);

    Task<string> ForgotPasswordAsync(string email);

    Task<ResponseDto<ApplicationUser>> ResetPasswordAsync(ResetPasswordDto model);

    Task<string> SendLoginDetailsEmail(LoginDataDto loginData);
}