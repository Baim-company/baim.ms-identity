using Global.Infrastructure.Exceptions.Identity;
using Identity.API.Data.Dtos.ExternalUsers;
using Identity.API.Data.Dtos.Password;
using Identity.API.Data.Dtos.Register;
using Identity.API.Services.Abstractions;
using Identity.API.Services.Validations;
using Identity.API.Data.Dtos.Email;
using Identity.API.Data.Dtos.Responses;
using Identity.API.Data.Dtos.Login;
using System.IdentityModel.Tokens.Jwt;
using Identity.API.Data.Dtos.Url;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Identity.API.Data.DbContexts;
using Identity.API.Data.Entities;
using System.Data;


namespace Identity.API.Services.Implementations;


public class AccountService : IAccountService
{

    private readonly UrlSettings _urlSettings;
    private readonly AuthDbContext _authDbContext;
    private readonly IExternalUserSyncService _externalUserSyncService;
    private readonly IPasswordService _passwordService;
    private readonly IEmailService _emailService;
    private readonly IFileService _fileService;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;


    public AccountService(
        AuthDbContext authDbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        UrlSettings urlSettings,
        IEmailService emailService,
        IFileService fileService,
        ITokenService tokenService,
        IPasswordService passwordService,
        IConfiguration configuration,
        IExternalUserSyncService externalUserSyncService,
        ILogger<AccountService> logger)
    {
        _authDbContext = authDbContext;
        _roleManager = roleManager;
        _userManager = userManager;
        _emailService = emailService;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _externalUserSyncService = externalUserSyncService;
        _configuration = configuration;
        _logger = logger;
        _urlSettings = urlSettings;
        _fileService = fileService;

    }





    public async Task<ResponseDto<ApplicationUser>> ChangePasswordAsync(ChangePasswordDto model)
    {
        try
        {
            _logger.LogInformation("Starting password change process for user {Email}", model.Email);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning("User with email {Email} not found", model.Email);
                throw new IdentityException(IdentityErrorType.UserNotFound, "User not found");
            }

            if (!AuthValidation.CheckPassword(model.NewPassword))
            {
                _logger.LogWarning("Password does not meet security criteria for user {Email}", model.Email);
                return ErrorResponse("Password must be more than 6 and less than 40 characters long and include a special symbol");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Password change failed for user {Email}: {Errors}", model.Email, errorMessages);
                return ErrorResponse($"Password change failed: {errorMessages}");
            }


            _logger.LogInformation("Password changed successfully for user {Email}", model.Email);
            return SuccessResponse(new ApplicationUser(), "Password changed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while changing the password for user {Email}", model.Email);
            throw;
        }
    }





    public async Task<string> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
            throw new IdentityException(IdentityErrorType.UserNotFound, $"User with email: {email} not found");
        }
        var resetToken = _tokenService.GenerateResetPasswordToken();
        await SaveResetTokenAsync(user.Id, resetToken);

        var resetPasswordUrl = $"{_urlSettings.ResetPasswordPage}?token={Uri.EscapeDataString(resetToken)}";

        try
        {

            SendResetPasswordEmail(user.Email!, resetPasswordUrl);

            _logger.LogInformation("Reset password URL successfully has been sent for user {Email}", user.Email);
            return $"Reset password URL successfully has been sent for user {user.Email}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending reset password email for user {Email}", user.Email);
            throw;
        }
    }

    private async Task SaveResetTokenAsync(Guid userId, string resetToken)
    {
        var passwordResetEntry = new PasswordResetToken
        {
            UserId = userId,
            Token = resetToken,
            ExpiryDate = DateTime.UtcNow.AddHours(1)
        };
        _authDbContext.PasswordResetTokens.Add(passwordResetEntry);
        await _authDbContext.SaveChangesAsync();
    }

    private void SendResetPasswordEmail(string email, string resetPasswordUrl)
    {
        var message = new EmailMessageDto(new[] { email }, "Reset password link", resetPasswordUrl)
        {
            HtmlFilePath = "./wwwroot/HTML Letters/Forgot password/index.html"
        };
        _emailService.SendEmail(message, resetPasswordUrl, null, null);
    }







    public async Task<ResponseDto<ApplicationUser>> ResetPasswordAsync(ResetPasswordDto model)
    {
        try
        {
            var decodedToken = _tokenService.DecodeToken(model.Token);
            var passwordResetEntry = await _authDbContext.PasswordResetTokens.FirstOrDefaultAsync(p => p.Token == decodedToken);

            if (passwordResetEntry == null || passwordResetEntry.ExpiryDate < DateTime.UtcNow)
                return ErrorResponse("Invalid or expired token!");



            var user = await _authDbContext.Users.FindAsync(passwordResetEntry.UserId);

            if (user == null)
                return ErrorResponse("User not found");
            if (model.Password != model.ConfirmPassword)
                return ErrorResponse("Passwords do not matc");


            ResetUserPassword(user, model.Password);
            await RemovePasswordResetEntryAsync(passwordResetEntry);


            _logger.LogInformation("Password reset successfully for user {Email}", user.Email);
            return SuccessResponse(user, "Password reset successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while resetting the password");
            throw new InvalidOperationException("Failed to reset password");
        }
    }

    private void ResetUserPassword(ApplicationUser user, string newPassword)
    {
        var passwordHasher = new PasswordHasher<ApplicationUser>();
        user.PasswordHash = passwordHasher.HashPassword(user, newPassword);

    }

    private async Task RemovePasswordResetEntryAsync(PasswordResetToken token)
    {
        _authDbContext.PasswordResetTokens.Remove(token);
        await _authDbContext.SaveChangesAsync();
    }




     



    public async Task<ResponseDto<ApplicationUser>> RegisterAsync(RegisterUserDto model, bool isOldClient)
    {
        if (!await _roleManager.RoleExistsAsync(model.Role))
            return ErrorResponse($"Role {model.Role} doesn't exist!");

        if (!AuthValidation.CheckEmail(model.Email))
            return ErrorResponse("Invalid email format!");

        if (await _userManager.FindByEmailAsync(model.Email) != null)
            return ErrorResponse($"User with email: {model.Email} already exists");



        var password = isOldClient ? 
            _configuration["OldClientSettings:Password"] ?? throw new Exception("Old client password is not configured properly in the configuration.")
            : _passwordService.GenerateRandomPassword();

        var newUser = new ApplicationUser(model, isOldClient);
        newUser.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(newUser, password);


        newUser = _fileService.SetDefaultFile(newUser,model.Role);

        if (!await SyncExternalUserAsync(newUser, model))
            return ErrorResponse($"Error! Failed to create {model.Role} model!");

        if (!await ConfirmEmailAndCreateUserAsync(newUser, password, model.Role, isOldClient))
            return ErrorResponse($"Failed to create {model.Role}.");


        await _authDbContext.SaveChangesAsync();

        return SuccessResponse(newUser, $"{model.Role} created and email sent successfully.");
    }

        


    private async Task<bool> ConfirmEmailAndCreateUserAsync(ApplicationUser user, string password, string role, bool isOldClient)
    {
        var token = _tokenService.GenerateEmailConfirmationToken(user);
        if (string.IsNullOrEmpty(token)) return false;

        var confirmationLink = GenerateConfirmationLink(token);
        if (string.IsNullOrEmpty(confirmationLink)) return false;

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded) return false;

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded) return false;

        if (!isOldClient)
            SendConfirmationEmail(user.Email!, confirmationLink, password);

        return true;
    }

    private async Task<bool> CreateUserAsync(ApplicationUser user, string password, string role)
    {
        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded) return false;

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        return roleResult.Succeeded;
    } 

    private async Task<bool> SyncExternalUserAsync(ApplicationUser user, RegisterUserDto model)
    {
        return model.Role switch
        {
            "UserAdmin" => await _externalUserSyncService.AddClientAdminAsync(new ExternalUserDto(user, model.Role)) != null,
            "User" => model.CompanyId != Guid.Empty &&
                      await _externalUserSyncService.AddClientAsync(new ExternalUserCompanyDto(new ExternalUserDto(user, model.Role), model.CompanyId)) != null,
            "Staff" => await _externalUserSyncService.AddStaffAsync(new ExternalUserDto(user, model.Role)) != null,
            _ => false
        };
    }

    private string GenerateConfirmationLink(string token) => $"{_urlSettings.BasePath}/ConfirmEmail?token={token}";
    private void SendConfirmationEmail(string email, string confirmationLink, string password)
    {
        var message = new EmailMessageDto(new[] { email }, "Confirmation your email", confirmationLink)
        {
            HtmlFilePath = "./wwwroot/HTML Letters/Confirm email/index.html"
        };
        _emailService.SendEmail(message, confirmationLink, password, email);
    }







    public async Task<bool> ConfirmEmailAsync(string token)
    {
        var validatedToken = _tokenService.ValidateToken(token);
        if (validatedToken == null) return false;

        var jwtToken = (JwtSecurityToken)validatedToken;
        var userId = jwtToken.Claims.First(x => x.Type == "id").Value;

        var user = await _authDbContext.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
        if (user == null) throw new IdentityException(IdentityErrorType.UserNotFound, "User not found");


        if (await _userManager.IsEmailConfirmedAsync(user))
            throw new IdentityException(IdentityErrorType.EmailAlreadyConfirmed, "Email already confirmed");

        user.EmailConfirmed = true;
        await _authDbContext.SaveChangesAsync();

        _logger.LogInformation("Email confirmed successfully for user {Email}", user.Email);

        return true;
    }







    public async Task<string> SendLoginDetailsEmail(LoginDataDto loginData)
    {
        var user = await _userManager.FindByEmailAsync(loginData.login);
        if (user == null)
        {
            _logger.LogWarning("User with login {Login} doesn't exist", loginData.login);
            throw new IdentityException(IdentityErrorType.UserNotFound, $"User with login: {loginData.login} doesn't exist!");
        }

        var message = new EmailMessageDto(new[] { loginData.email }, "Your account data")
        {
            HtmlFilePath = "./wwwroot/HTML Letters/Old client access data/index.html"
        };

        try
        {
            _emailService.SendEmail(message, loginData.password, loginData.login);
            _logger.LogInformation("Login details successfully sent to email: {Email}", loginData.email);
            return $"Login details successfully were sent to email: {loginData.email}.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send login details email to {Email}", loginData.email);
            throw new IdentityException(IdentityErrorType.InvalidRequest, $"Could not send link to email, please try again. Message: {ex.Message}");
        }
    }



    private ResponseDto<ApplicationUser> ErrorResponse(string message) => new() { Message = message };
    private ResponseDto<ApplicationUser> SuccessResponse(ApplicationUser user, string message) => new() { Data = user, Message = message };
}