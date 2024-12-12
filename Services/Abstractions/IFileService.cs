using Identity.API.Data.Dtos.File;
using Identity.API.Data.Dtos.Responses;
using Identity.API.Data.Entities;

namespace Identity.API.Services.Abstractions;

public interface IFileService
{
    Task<ResponseDto<FileDto>> GetFileByIdAsync(Guid userId);
    Task<ResponseDto<string>> UpdateFileAsync(Guid userId, IFormFile newFile);
    Task<ResponseDto<bool>> DeleteFileAsync(Guid userId);
    //Task<ResponseDto<bool>> CreateFileAsync(Guid userId, IFormFile newFile);

    ApplicationUser SetDefaultFile(ApplicationUser user,string role = "Admin");

    public bool IsDefaultImage(string fileName);
    public string GetRootPath();
}