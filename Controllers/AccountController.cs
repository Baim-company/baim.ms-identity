using Identity.API.Data.Dtos.Login;
using Identity.API.Data.Dtos.Password;
using Identity.API.Data.Dtos.Register;
using Identity.API.Data.Dtos.Url;
using Identity.API.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Identity.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{

    private readonly UrlSettings _urlSettings;
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService,UrlSettings urlSettings)
    {
        _accountService = accountService;
        _urlSettings = urlSettings;
    }


    [Authorize(Policy = "AdminOnly")]
    [HttpPost("register")]
    public async Task<ActionResult<string>> Create([FromBody] RegisterUserDto model, [FromHeader] bool isOldClient)
    {
        var result = await _accountService.RegisterAsync(model, isOldClient);
        if (result.Data == null) return BadRequest(result.Message);

        return Ok(result.Message);
    }





    [HttpGet("confirmEmail")]
    public async Task<IActionResult> ConfirmEmail(string token)
    {
        var isConfirmed = await _accountService.ConfirmEmailAsync(token);
        return isConfirmed ? Redirect(_urlSettings.EmailConfirmedPage) : BadRequest("Invalid Token");
    }




    [Authorize(Policy = "AdminAndStaffOnly")]
    [HttpPost("sendEmail")]
    public async Task<ActionResult<string>> SendLoginDetailsEmail([FromBody] LoginDataDto model)
    {
        var result = await _accountService.SendLoginDetailsEmail(model);

        return Ok(result);
    }





    [Authorize]
    [HttpPut("changePassword")]
    public async Task<ActionResult<string>> ChangePassword([FromBody] ChangePasswordDto model)
    {
        var result = await _accountService.ChangePasswordAsync(model);
        if (result.Data == null) return BadRequest(result.Message);

        return Ok(result.Message);
    }




    [AllowAnonymous]
    [HttpPost("forgotPassword")]
    public async Task<ActionResult<string>> ForgotPassword([FromHeader] string email)
    {
        var result = await _accountService.ForgotPasswordAsync(email);

        return Ok(result);
    }




    [AllowAnonymous]
    [HttpPost("resetPassword")]
    public async Task<ActionResult<string>> ResetPassword([FromBody] ResetPasswordDto model)
    {
        var result = await _accountService.ResetPasswordAsync(model);
        if (result.Data == null) return BadRequest(result.Message);

        return Ok(result.Message);
    }
}