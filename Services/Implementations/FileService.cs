using Identity.API.Data.DbContexts;
using Identity.API.Data.Dtos.File;
using Identity.API.Data.Dtos.Responses;
using Identity.API.Data.Entities;
using Identity.API.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


public class FileService : IFileService
{

    private readonly string _imagesRootPath;
    private readonly AuthDbContext _authDbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IExternalUserPhotoSyncService _externalUserPhotoSyncService;
    private readonly HashSet<string> _defaultImages = new()
    {
        "admin-icon.png",
        "staff-icon.png",
        "user-icon.png",
        "user-admin-icon.png"
    };


    public FileService(AuthDbContext authDbContext,
        UserManager<ApplicationUser> userManager,
        IExternalUserPhotoSyncService externalUserPhotoSyncService,
        IConfiguration configuration)
    {
        _authDbContext = authDbContext;
        _userManager = userManager;
        _imagesRootPath = configuration["ImagesRootPath"]
            ?? throw new Exception("ImagesRootPath is not configured");
        _externalUserPhotoSyncService = externalUserPhotoSyncService;
    }


    
    public async Task<ResponseDto<FileDto>> GetFileByIdAsync(Guid userId)
    {
        var user = await _authDbContext.Users.FindAsync(userId);
        if (user == null)
            return new ResponseDto<FileDto>($"File with user id: {userId} not found.");

        var filePath = Path.Combine(_imagesRootPath, user.AvatarPath);
        if (!File.Exists(filePath))
            return new ResponseDto<FileDto>($"File not found at path: {filePath}");

        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var contentType = GetContentType(filePath);

        return new ResponseDto<FileDto>("Success", new FileDto
        {
            FileName = user.AvatarPath,
            ContentType = contentType,
            FileContent = fileBytes
        });
    }



    // app/images
    public async Task<ResponseDto<string>> UpdateFileAsync(Guid userId, IFormFile newFile)
    {
        if (newFile == null || newFile.Length == 0)
            return new ResponseDto<string>("New file is empty or not provided.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return new ResponseDto<string>("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        var roleFolder = role switch
        {
            "Admin" => "admin",
            "UserAdmin" => "userAdmin",
            "Staff" => "staff",
            _ => "user"
        };

        var targetDirectory = Path.Combine(_imagesRootPath, roleFolder);

        if (!Directory.Exists(targetDirectory))
            Directory.CreateDirectory(targetDirectory);

        var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(newFile.FileName)}";
        var newFilePath = Path.Combine(targetDirectory, newFileName);

        await using var stream = File.Create(newFilePath);
        await newFile.CopyToAsync(stream);


        // Удаление предыдущего файла, если он не является дефолтным
        if (!IsDefaultImage(user.AvatarName))
        {
            var previousFilePath = Path.Combine(_imagesRootPath, user.AvatarPath);
            if (File.Exists(previousFilePath)) File.Delete(previousFilePath);
        }

        user.AvatarName = newFileName;
        user.AvatarPath = Path.Combine(roleFolder, newFileName);

        var result = await _externalUserPhotoSyncService.UpdatePhotoAsync(userId, user.AvatarPath); 
        if (result == null) return new ResponseDto<string>("Error! Failed to update sync image in Personal Account microservice.");

        await _userManager.UpdateAsync(user);

        return new ResponseDto<string>("File updated successfully!", user.AvatarPath);
    }



    public async Task<ResponseDto<bool>> DeleteFileAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return new ResponseDto<bool>("User not found.", false);

        if (_defaultImages.Contains(user.AvatarName))
            return new ResponseDto<bool>($"Cannot delete default image: {user.AvatarName}.", false);

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        var defaultFileName = role switch
        {
            "Admin" => "admin-icon.png",
            "UserAdmin" => "user-admin-icon.png",
            "Staff" => "staff-icon.png",
            _ => "user-icon.png"
        };

        var defaultFilePath = Path.Combine("default", defaultFileName);

        var filePath = Path.Combine(_imagesRootPath, user.AvatarPath);
        if (File.Exists(filePath))
            File.Delete(filePath);

        user.AvatarName = defaultFileName;
        user.AvatarPath = defaultFilePath;


        var result = await _externalUserPhotoSyncService.DeletePhotoAsync(userId, user.AvatarPath); 
        if (result == null) return new ResponseDto<bool>("Error! Failed to update sync image in Personal Account microservice.", false);

        await _userManager.UpdateAsync(user);

        return new ResponseDto<bool>("File deleted successfully, default image set.", true);
    }





    public ApplicationUser SetDefaultFile(ApplicationUser user, string role = "Admin")
    {
        string fileName = role switch
        {
            "Admin" => "admin-icon.png",
            "UserAdmin" => "user-admin-icon.png",
            "Staff" => "staff-icon.png",
            _ => "user-icon.png"
        };

        user.AvatarPath = Path.Combine($"default", fileName);
        user.AvatarName = fileName;

        return user;
    }


    public bool IsDefaultImage(string fileName) => _defaultImages.Contains(fileName);
    public string GetRootPath() => _imagesRootPath;


    private string GetContentType(string path)
    {
        var types = new Dictionary<string, string>
        {
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".bmp", "image/bmp" },
            { ".svg", "image/svg+xml" }
        };

        return types.TryGetValue(Path.GetExtension(path).ToLowerInvariant(), out var contentType)
            ? contentType
            : "application/octet-stream";
    }
}
