using Identity.API.Data.Dtos.Responses;
using Identity.API.Data.Dtos.Settings;
using Identity.API.Data.Entities;

namespace Identity.API.Services.Abstractions;

public interface ISettingsService
{
    Task<ResponseDto<ApplicationUser>> UpdateUserSettingsAsync(SettingsDto settingsDto);
}