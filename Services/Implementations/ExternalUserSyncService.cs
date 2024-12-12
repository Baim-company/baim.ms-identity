using Identity.API.Data.Dtos.ExternalUsers;
using Identity.API.Services.Abstractions;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;


namespace Identity.API.Services.Implementations;

public class ExternalUserSyncService : IExternalUserSyncService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenService _tokenService;

    public ExternalUserSyncService(HttpClient httpClient, ITokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
    }



    // Done
    private async Task AddAuthorizationHeaderAsync(HttpRequestMessage request)
    {
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }


    // Done
    public async Task<string?> AddAdminAsync(ExternalUserDto userModel,string? token)
    {
        var jsonContent = new StringContent(JsonSerializer.Serialize(userModel), Encoding.UTF8, "application/json");
        
        var request = new HttpRequestMessage(HttpMethod.Post, "/Admin/Create")
        {
            Content = jsonContent
        };

        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);


        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync();

        return null;
    }




    public async Task<string?> AddClientAsync(ExternalUserCompanyDto externalUserCompanyDto)
    {
        var jsonContent = new StringContent(JsonSerializer.Serialize(externalUserCompanyDto), Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "/Client/Create/Client")
        {
            Content = jsonContent
        };
        await AddAuthorizationHeaderAsync(request);


        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Error with creating user: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        return null;
    }





    // Done
    public async Task<string?> AddClientAdminAsync(ExternalUserDto externalUserDto)
    {
        var jsonContent = new StringContent(JsonSerializer.Serialize(externalUserDto), Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "/Client/Create/ClientAdmin")
        {
            Content = jsonContent
        };
        await AddAuthorizationHeaderAsync(request);


        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Error with creating user admin: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        return null;
    }


    // Done
    public async Task<string?> UpdateClientDataAsync(ExternalUserDto updateExternalUserDto)
    {
        var jsonContent = new StringContent(JsonSerializer.Serialize(updateExternalUserDto), Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Put, "/Client/Update")
        {
            Content = jsonContent
        };
        await AddAuthorizationHeaderAsync(request);


        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Error with updating client: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        return null;
    }








    // Done
    public async Task<string?> AddStaffAsync(ExternalUserDto userModel)
    {
        var jsonContent = new StringContent(JsonSerializer.Serialize(userModel), Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "/Staff/Create")
        {
            Content = jsonContent
        };
        await AddAuthorizationHeaderAsync(request);


        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Error with creating staff: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        return null;
    }
    

    // Done
    public async Task<string?> UpdateStaffDataAsync(ExternalUserDto updateExternalUserDto)
    {
        var jsonContent = new StringContent(JsonSerializer.Serialize(updateExternalUserDto), Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Put, "/Staff/Update")
        {
            Content = jsonContent
        };
        await AddAuthorizationHeaderAsync(request);


        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Error with updating staff: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        return null;
    }
}