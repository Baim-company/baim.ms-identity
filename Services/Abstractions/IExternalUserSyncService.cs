using Identity.API.Data.Dtos.ExternalUsers;

namespace Identity.API.Services.Abstractions;

public interface IExternalUserSyncService
{
    public Task<string?> AddClientAsync(ExternalUserCompanyDto externalUserCompanyDto);
    public Task<string?> AddClientAdminAsync(ExternalUserDto externalUserDto);
    public Task<string?> AddStaffAsync(ExternalUserDto externalUserDto);
    public Task<string?> AddAdminAsync(ExternalUserDto externalUserDto,string token);


    public Task<string?> UpdateClientDataAsync(ExternalUserDto updateExternalUserDto);
    public Task<string?> UpdateStaffDataAsync(ExternalUserDto updateExternalUserDto);
}