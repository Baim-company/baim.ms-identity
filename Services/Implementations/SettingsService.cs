using Global.Infrastructure.Exceptions.Identity;
using Identity.API.Data.DbContexts;
using Identity.API.Data.Dtos.ExternalUsers;
using Identity.API.Data.Dtos.Responses;
using Identity.API.Data.Dtos.Settings;
using Identity.API.Data.Entities;
using Identity.API.Services.Abstractions;
using Identity.API.Services.Validations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace Identity.API.Services.Implementations;

public class SettingsService : ISettingsService
{

    private readonly IMapper _mapper;
    private readonly IExternalUserSyncService _externalUserSyncService;
    private readonly AuthDbContext _authDbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IFileService _fileService;

    public SettingsService(
        AuthDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IExternalUserSyncService externalUserSyncService,
        IFileService fileService,
        IMapper mapper)
    {
        _mapper = mapper;
        _authDbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _fileService = fileService;
        _externalUserSyncService = externalUserSyncService;
    }




    public async Task<ResponseDto<ApplicationUser>> UpdateUserSettingsAsync(SettingsDto settingsDto)
    {

        var user = await _userManager.FindByIdAsync(settingsDto.Id.ToString());
        if (user == null) throw new IdentityException(IdentityErrorType.UserNotFound, $"User with id: {settingsDto.Id} not found!");

        if (!ValidateEmails(settingsDto)) return new ResponseDto<ApplicationUser>("Invalid email format!", null);
        user.HasCompletedSurvey = true;

        var roles = await _userManager.GetRolesAsync(user);

        var result = roles.Any(r => r == "User" || r == "UserAdmin")
            ? await UpdateUserSettingsAsync(settingsDto, user, true)
            : await UpdateUserSettingsAsync(settingsDto, user, false);

        return result;
    }


    private async Task<ResponseDto<ApplicationUser>> UpdateUserSettingsAsync(SettingsDto model, ApplicationUser user, bool isClient)
    {
        if (await IsEmailTakenAsync(model.Email, user.Email!))
            return new ResponseDto<ApplicationUser>($"Error!\n{(isClient ? "Client" : "Staff")} with email already exist!", null);

        ExternalUserDto externalUserDto = isClient
            ? new ExternalUserDto(model, "Client")
            : new ExternalUserDto(model, "Staff");
        
        var updateResult = isClient
            ? await _externalUserSyncService.UpdateClientDataAsync(externalUserDto)
            : await _externalUserSyncService.UpdateStaffDataAsync(externalUserDto);

        if (updateResult == null)
            return new ResponseDto<ApplicationUser>($"Error! Failed to update {(isClient ? "client" : "staff")} on personal account service!", null);

        UpdateUserData(model, user);
        await _authDbContext.SaveChangesAsync();

        return new ResponseDto<ApplicationUser>("Successfully updated!", user);
    }



    private void UpdateUserData(SettingsDto model, ApplicationUser user)
    {
        _mapper.Map(model, user);
        user.Gender = model.Gender;

        _authDbContext.Users.Update(user);
    }


    private async Task<bool> IsEmailTakenAsync(string email, string currentEmail)
    {
        return email != currentEmail && await _authDbContext.Users.AnyAsync(u => u.Email == email);
    }


    private bool ValidateEmails(SettingsDto model)
    {
        return AuthValidation.CheckEmail(model.Email) && AuthValidation.CheckEmail(model.PersonalEmail);
    }
}
