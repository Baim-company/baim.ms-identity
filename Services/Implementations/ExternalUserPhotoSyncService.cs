using Identity.API.Services.Abstractions;
using System.Net.Http.Headers;

namespace Identity.API.Services.Implementations;

public class ExternalUserPhotoSyncService : IExternalUserPhotoSyncService
{

    private readonly HttpClient _httpClient;
    private readonly ITokenService _tokenService; 

    public ExternalUserPhotoSyncService(HttpClient httpClient, ITokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
    }



    private async Task AddAuthorizationHeaderAsync(HttpRequestMessage request)
    {
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }




    public async Task<string?> DeletePhotoAsync(Guid userId, string imagePath)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/UserPhoto/User/Id/{userId}");
        request.Headers.Add("imagePath", imagePath);

        await AddAuthorizationHeaderAsync(request);

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Error deleting photo: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        return null;
    }




    public async Task<string?> UpdatePhotoAsync(Guid userId, string imagePath)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"/UserPhoto/User/Id/{userId}");
        request.Headers.Add("imagePath", imagePath);

        await AddAuthorizationHeaderAsync(request);

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Error updating photo: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        return null;
    }
}