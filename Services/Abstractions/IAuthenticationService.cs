using Identity.API.Data.Dtos.Login;
using Identity.API.Data.Dtos.Pagination;
using Identity.API.Data.Dtos.Responses;
using Identity.API.Data.Dtos.User;
using Identity.API.Data.Entities;

namespace Identity.API.Services.Abstractions;

public interface IAuthenticationService
{
    Task<AccessInfoDto> LoginAsync(LoginDto model);
    Task<AccessInfoDto> RefreshTokenAsync(TokenDto userAccessData);

    Task<PagedResponse<UserDto>> GetUsersAsync(PaginationParameters paginationParameters);
    Task<ResponseDto<UserDto>> GetUserAsync(Guid id);
}