using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace Project.Services.Authorization;

public class KeycloakTokenAccessor : ITokenAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public KeycloakTokenAccessor(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return null;

        return await context.GetTokenAsync("access_token");
    }

    public async Task SetAuthorizationHeaderAsync(HttpClient httpClient, bool isClient = false)
    {
        string? token;

        if (isClient)
        {
            token = await GetClientCredentialsTokenAsync();
        }
        else
        {
            token = await GetAccessTokenAsync();
        }

        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private async Task<string?> GetClientCredentialsTokenAsync()
    {
        var host = _configuration["Keycloak:Host"];
        var realm = _configuration["Keycloak:Realm"];
        var clientId = _configuration["Keycloak:ClientId"];
        var clientSecret = _configuration["Keycloak:ClientSecret"];

        var tokenEndpoint = $"{host}/realms/{realm}/protocol/openid-connect/token";

        var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId!,
            ["client_secret"] = clientSecret!
        });

        var response = await _httpClient.PostAsync(tokenEndpoint, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content);

        return tokenResponse?.AccessToken;
    }

    private class TokenResponse
    {
        public string? AccessToken { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string? access_token 
        { 
            set => AccessToken = value; 
        }
    }
}
