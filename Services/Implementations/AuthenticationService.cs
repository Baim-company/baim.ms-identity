using AutoMapper;
using Global.Infrastructure.Exceptions.Identity;
using Identity.API.Data.DbContexts;
using Identity.API.Data.Dtos.Login;
using Identity.API.Data.Dtos.Pagination;
using Identity.API.Data.Dtos.Responses;
using Identity.API.Data.Dtos.Settings;
using Identity.API.Data.Dtos.User;
using Identity.API.Data.Entities;
using Identity.API.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Identity.API.Services.Implementations;


public class AuthenticationService : IAuthenticationService
{

    private readonly IMapper _mapper;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly UserManager<ApplicationUser> _userManager; 


    public AuthenticationService(
        UserManager<ApplicationUser> userManager, 
        ITokenService tokenService, 
        ILogger<AuthenticationService> logger,
        IMapper mapper)
    {
        _mapper = mapper;
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
    }



    public async Task<ResponseDto<UserDto>> GetUserAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user == null) return new ResponseDto<UserDto>() { Message = $"There is no user with id: {id}" };

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "No Role";

        var responseUser = _mapper.Map<UserDto>(user);
        responseUser.Role = role;
            
        return new ResponseDto<UserDto>() { Data = responseUser, Message = "Success ." };
    }



    public async Task<PagedResponse<UserDto>> GetUsersAsync(PaginationParameters paginationParameters)
    {
        var usersQuery = _userManager.Users
                .AsNoTracking()
                .AsQueryable();

        var totalRecords = await usersQuery.CountAsync();

        var paginatedUsers = await usersQuery
            .Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
            .Take(paginationParameters.PageSize)
            .ToListAsync();

        var userDtos = new List<UserDto>();
        foreach (var user in paginatedUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "No Role";

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Role = role;

            userDtos.Add(userDto);
        }

        return new PagedResponse<UserDto>(
            userDtos, 
            paginationParameters.PageNumber, 
            paginationParameters.PageSize, 
            totalRecords);
    }

     




    public async Task<AccessInfoDto> LoginAsync(LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            throw new IdentityException(IdentityErrorType.UserNotFound,$"User with email: {model.Email} not found!");

        if (!user.EmailConfirmed)
            throw new IdentityException(IdentityErrorType.EmailNotConfirmed, $"Confirme your email.");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!isPasswordValid)
            throw new IdentityException(IdentityErrorType.PasswordMismatch, $"Incorrect password .");

        try
        {
            var accessToken = await _tokenService.GenerateTokenAsync(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            var accessInfo = new AccessInfoDto(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                TokenExpireTime: DateTime.UtcNow.AddHours(1)
            );

            return accessInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during login for user {Email}", model.Email);
            throw new InvalidOperationException("An error occurred during login.");
        }
    }




    public async Task<AccessInfoDto> RefreshTokenAsync(TokenDto userAccessData)
    {
        if (userAccessData is null) 
            throw new IdentityException(IdentityErrorType.InvalidRequest, "Invalid client request");

        var accessToken = userAccessData.AccessToken;
        var refreshToken = userAccessData.RefreshToken;

        var principal = _tokenService.GetPrincipalFromToken(accessToken);

        var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            throw new IdentityException(IdentityErrorType.InvalidToken, "Invalid user ID in token");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new IdentityException(IdentityErrorType.UserNotFound, "User not found");

        if (user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new IdentityException(IdentityErrorType.InvalidRequest, "Invalid refresh token or token expired");

        var newAccessToken = await _tokenService.GenerateTokenAsync(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken(); 
        var tokenExpireTime = DateTime.UtcNow.AddMinutes(30);

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            throw new IdentityException(IdentityErrorType.InvalidRequest, $"Failed to update user: {errors}");
        }

        return new AccessInfoDto(
            newAccessToken,
            newRefreshToken,
            tokenExpireTime);
    }
}