using Identity.API.Services.Abstractions;
using Identity.API.Data.Dtos.Login;
using Microsoft.AspNetCore.Identity;
using Identity.API.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Identity.API.Data.Dtos.Pagination;
using Identity.API.Data.Dtos.Responses;
using Microsoft.AspNetCore.Authorization;
using Identity.API.Data.Dtos.User;


namespace Identity.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{

    private readonly IAuthenticationService _authenticationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthenticationController(IAuthenticationService authenticationService,
        UserManager<ApplicationUser> userManager)
    {
        _authenticationService = authenticationService;
        _userManager = userManager;
    }




    [HttpPost("Login")]
    public async Task<ActionResult<AccessInfoDto>> LoginAsync([FromBody] LoginDto model)
    {
        var result = await _authenticationService.LoginAsync(model);

        return Ok(result);
    }



    //[Authorize]
    //[HttpPost("Refresh")]
    //public async Task<IActionResult> RefreshTokenAsync(TokenDto refresh)
    //{
    //    var newToken = await _authenticationService.RefreshTokenAsync(refresh);

    //    if (newToken is null) return BadRequest("Invalid token");

    //    return Ok("Successfully refreshed!");
    //}



    
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("Users")]
    public async Task<ActionResult<PagedResponse<UserDto>>> GetAllUsers([FromQuery] PaginationParameters paginationParameters)
    {
        var result = await _authenticationService.GetUsersAsync(paginationParameters);

        return Ok(result);
    }
    


    [Authorize(Policy = "AdminOnly")]
    [HttpGet("User/{id}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var result = await _authenticationService.GetUserAsync(id);

        if (result.Data == null) return NotFound($"There is no user with id: {id} .Error message: {result.Message}");

        return Ok(result.Data);
    }
}