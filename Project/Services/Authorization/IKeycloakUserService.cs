namespace Project.Services.Authorization;

public interface IKeycloakUserService
{
    /// <summary>
    /// Создает нового пользователя в Keycloak
    /// </summary>
    Task<(bool Success, string? Error)> CreateUserAsync(string email, string password, string? avatarUrl);
}
