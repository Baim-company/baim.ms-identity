using Identity.API.Data.DbContexts;
using Identity.API.Data.Entities;
using Identity.API.Data.Enums;
using Identity.API.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace Identity.API.Services.Implementations;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly AuthDbContext _authDbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public TokenService(IConfiguration config,
        AuthDbContext authDbContext,
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _configuration = config;
        _userManager = userManager;
        _authDbContext = authDbContext;
        _httpContextAccessor = httpContextAccessor;
    }


    public Task<string?> GetTokenAsync()
    {
        var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(authorizationHeader))
            return Task.FromResult<string?>(null);

        return Task.FromResult(authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorizationHeader.Substring(7)
            : null);
    }

    public string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }


    public async Task<string> GenerateTokenAsync(ApplicationUser user)
    {
        try
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Value!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);

            var gender = user.Gender switch
            {
                Gender.Man => "Man",
                Gender.Woman => "Woman",
                _ => "Unknown"
            };

            var claims = new List<Claim>
            {
                new Claim("Id", user.Id.ToString()),
                new Claim("Email", user.Email ?? string.Empty),
                new Claim("Name", user.Name ?? string.Empty),
                new Claim("Surname", user.Surname ?? string.Empty),
                new Claim("MobilePhone", user.PhoneNumber ?? string.Empty),
                new Claim("Gender", gender),
                new Claim("HasCompletedSurvey", user.HasCompletedSurvey.ToString()),
                new Claim("DateOfBirth", user.BirthDate.ToString("yyyy-MM-dd") ?? string.Empty),
                new Claim("PersonalEmail", user.PersonalEmail ?? string.Empty),
                new Claim("Patronymic", user.Patronymic ?? string.Empty),
                new Claim("BusinessPhoneNumber", user.BusinessPhoneNumber ?? string.Empty),
                new Claim("EmailConfirmed", user.EmailConfirmed.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(roles
                .Select(role => new Claim("Role", role)));


            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("Jwt:Issuer").Value,
                audience: _configuration.GetSection("Jwt:Audience").Value,
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public ClaimsPrincipal GetPrincipalFromToken(string token, bool validateLifetime = false)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Value!)),
            ValidateLifetime = validateLifetime
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        SecurityToken securityToken;

        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

        var jwtSecurityToken = securityToken as JwtSecurityToken;

        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals("HS256"))
            throw new SecurityTokenException("Invalid token");
        return principal;
    }

    public SecurityToken? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return validatedToken;
        }
        catch (Exception ex)
        {
            return null;
        }
    }



    public string GenerateEmailConfirmationToken(ApplicationUser user)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(10),
                Issuer = _configuration.GetSection("Jwt:Issuer").Value,
                Audience = _configuration.GetSection("Jwt:Audience").Value,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception)
        {
            throw;
        }
    }


    public string GenerateResetPasswordToken()
    {
        using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
        {
            var randomBytes = new byte[32];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }

    public string DecodeToken(string token)
    {
        return Uri.UnescapeDataString(token);
    }
}
