namespace Project.Services.Authorization;

public interface ITokenAccessor
{
    /// <summary>
    /// Устанавливает заголовок авторизации для HTTP-клиента
    /// </summary>
    /// <param name="httpClient">HTTP-клиент</param>
    /// <param name="isClient">true - токен клиента, false - токен пользователя</param>
    Task SetAuthorizationHeaderAsync(HttpClient httpClient, bool isClient = false);
    
    /// <summary>
    /// Получает токен доступа пользователя
    /// </summary>
    Task<string?> GetAccessTokenAsync();
}
