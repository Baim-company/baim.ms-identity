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

    private readonly ISettingsService _settingsService;
    private readonly IFileService _fileService;

    public SettingsController(ISettingsService settingsService, IFileService fileService)
    {
        _settingsService = settingsService;
        _fileService = fileService;
    }


    [HttpPut("saveChanges")]
    public async Task<ActionResult<string>> Settings([FromBody] SettingsDto model)
    {
        var response = await _settingsService.UpdateUserSettingsAsync(model);

        if (response.Data == null)
            return BadRequest(response.Message);

        return Ok(response.Message);
    }


    [HttpPut("avatarImage/userId/{userId}")]
    public async Task<ActionResult<string>> UpdateFile(Guid userId, IFormFile file)
    {
        var result = await _fileService.UpdateFileAsync(userId, file);
        if (result.Data == null)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }


    [HttpDelete("avatarImage/userId/{userId}")]
    public async Task<ActionResult<string>> DeleteFile(Guid userId)
    {
        var result = await _fileService.DeleteFileAsync(userId);
        if (!result.Data)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }



    [HttpGet("avatarImage/userId/{userId}")]
    public async Task<ActionResult<FileContentResult>> GetFile(Guid userId)
    {
        var result = await _fileService.GetFileByIdAsync(userId);
        if (result.Data == null)
            return NotFound(result.Message);

        var file = result.Data;
        return File(file.FileContent, file.ContentType, file.FileName);
    }
}