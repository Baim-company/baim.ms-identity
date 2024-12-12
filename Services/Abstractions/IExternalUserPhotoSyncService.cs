namespace Identity.API.Services.Abstractions;

public interface IExternalUserPhotoSyncService
{
    Task<string?> UpdatePhotoAsync(Guid userId, string imagePath);
    Task<string?> DeletePhotoAsync(Guid userId, string imagePath);
}