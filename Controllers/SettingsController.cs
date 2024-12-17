using Identity.API.Data.Dtos.Settings;
using Identity.API.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Identity.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class SettingsController : ControllerBase
{

    private readonly IFileService _fileService;
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService, 
        IFileService fileService)
    {
        _settingsService = settingsService;
        _fileService = fileService;
    }



    [HttpGet("AvatarImage/User/Id/{userId}")]
    public async Task<IActionResult> GetFile(Guid userId)
    {
        var result = await _fileService.GetFileByIdAsync(userId);
        if (result.Data == null)
            return NotFound(result.Message);

        var file = result.Data;
        return File(file.FileContent, file.ContentType, file.FileName);
    }




    [HttpPut("SaveChanges")]
    public async Task<ActionResult<string>> Settings([FromBody] SettingsDto model)
    {
        var response = await _settingsService.UpdateUserSettingsAsync(model);

        if (response.Data == null)
            return BadRequest(response.Message);

        return Ok(response.Message);
    }

    [Authorize(Policy = "UserOnly")]
    [HttpPatch("AvatarImage/User/{userId}")]
    public async Task<ActionResult<string>> UpdateAvatarAsync([FromRoute] Guid userId, [FromForm] IFormFile file)
    {
        var result = await _fileService.UpdateFileAsync(userId, file);

        if (result.Data == null)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }




    [HttpDelete("AvatarImage/User/Id/{userId}")]
    public async Task<ActionResult<string>> DeleteFile(Guid userId)
    {
        var result = await _fileService.DeleteFileAsync(userId);
        if (!result.Data)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }
}