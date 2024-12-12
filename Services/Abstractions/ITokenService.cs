using Identity.API.Data.Entities;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Identity.API.Services.Abstractions;

public interface ITokenService
{ 
    string GenerateRefreshToken();
    Task<string> GenerateTokenAsync(ApplicationUser user);
    Task<string?> GetTokenAsync();


    ClaimsPrincipal GetPrincipalFromToken(string token, bool validateLifetime = false);
    SecurityToken? ValidateToken(string token);


    string GenerateEmailConfirmationToken(ApplicationUser user);
    string GenerateResetPasswordToken();
    string DecodeToken(string token);
}