using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Project.Services.Authorization;

public class KeycloakUserService : IKeycloakUserService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeycloakUserService> _logger;

    public KeycloakUserService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<KeycloakUserService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error)> CreateUserAsync(string email, string password, string? avatarUrl)
    {
        try
        {
            var token = await GetAdminTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return (false, "Failed to get admin token");
            }

            var host = _configuration["Keycloak:Host"];
            var realm = _configuration["Keycloak:Realm"];
            var usersEndpoint = $"{host}/admin/realms/{realm}/users";

            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);

            var userRepresentation = new
            {
                username = email,
                email = email,
                enabled = true,
                emailVerified = true,
                attributes = new Dictionary<string, string[]>
                {
                    ["avatar"] = new[] { avatarUrl ?? "Images/default-profile-picture.png" }
                },
                credentials = new[]
                {
                    new
                    {
                        type = "password",
                        value = password,
                        temporary = false
                    }
                }
            };

            var json = JsonSerializer.Serialize(userRepresentation);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(usersEndpoint, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("User {Email} created successfully", email);
                return (true, null);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create user {Email}. Status: {Status}, Error: {Error}", 
                email, response.StatusCode, errorContent);

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return (false, "Пользователь с таким email уже существует");
            }

            return (false, $"Ошибка создания пользователя: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception creating user {Email}", email);
            return (false, "Произошла ошибка при создании пользователя");
        }
    }

    private async Task<string?> GetAdminTokenAsync()
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
            _logger.LogError("Failed to get admin token. Status: {Status}", response.StatusCode);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        
        return doc.RootElement.GetProperty("access_token").GetString();
    }
}
