# Лабораторная работа №6: Аутентификация и авторизация

## Теория
В этой работе реализована интеграция с сервером аутентификации **Keycloak** для обеспечения безопасности веб-приложения. Keycloak — это open-source решение для Identity and Access Management (IAM), поддерживающее протоколы OAuth 2.0 и OpenID Connect.

### Ключевые понятия

- **Keycloak**: Сервер аутентификации и авторизации, предоставляющий страницу входа, регистрацию, выдачу и проверку токенов.
- **Realm**: Домен безопасности в Keycloak, содержащий пользователей, приложения, роли и политики доступа.
- **OpenID Connect (OIDC)**: Протокол аутентификации поверх OAuth 2.0, позволяющий клиентам верифицировать личность пользователя.
- **JWT (JSON Web Token)**: Компактный и самодостаточный способ передачи информации между сторонами в виде JSON-объекта с цифровой подписью.
- **Claims**: Утверждения о пользователе, содержащиеся в токене (email, роли, кастомные атрибуты).
- **Policy-based Authorization**: Авторизация на основе политик, позволяющая гибко определять правила доступа.

### Архитектура решения

```
[Браузер] <-> [Project (UI)] <-> [CPPR.API] <-> [PostgreSQL]
                  |                    |
                  +-----> [Keycloak] <-+
```

## Ход работы

### 1. Настройка Keycloak
- Используется существующий Keycloak сервер (`http://localhost:8080`)
- Создан Realm: `myapp-realm`
- Создан Client: `myapp-web` с поддержкой:
  - Client Authentication
  - Standard Flow (Authorization Code)
- Настроен маппер для передачи атрибута `avatar` в токен
- Создана роль `POWER-USER` для администраторов

### 2. Настройка API (CPPR.API)

#### Добавлен NuGet пакет:
```
Microsoft.AspNetCore.Authentication.JwtBearer
```

#### Конфигурация в `appsettings.json`:
```json
"AuthServer": {
  "Host": "http://localhost:8080",
  "Realm": "myapp-realm"
}
```

#### Настройка аутентификации в `Program.cs`:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MetadataAddress = $"{host}/realms/{realm}/.well-known/openid-configuration";
        options.Authority = $"{host}/realms/{realm}";
        options.Audience = "account";
        options.RequireHttpsMetadata = false;
    });

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("admin", p => p.RequireRole("POWER-USER"));
});
```

#### Защита эндпоинтов:
- Группа `/api/dish` защищена политикой `admin`
- GET-методы разрешены для анонимных пользователей (`AllowAnonymous()`)

### 3. Настройка UI (Project)

#### Добавлены NuGet пакеты:
```
Microsoft.AspNetCore.Authentication.OpenIdConnect
Microsoft.AspNetCore.Authentication.JwtBearer
```

#### Конфигурация в `appsettings.json`:
```json
"Keycloak": {
  "Host": "http://localhost:8080",
  "Realm": "myapp-realm",
  "ClientId": "myapp-web",
  "ClientSecret": "..."
}
```

#### Настройка OpenID Connect в `Program.cs`:
```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "keycloak";
})
.AddCookie()
.AddOpenIdConnect("keycloak", options =>
{
    options.Authority = $"{host}/realms/{realm}";
    options.ClientId = clientId;
    options.ClientSecret = clientSecret;
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.SaveTokens = true;
    // ...
});
```

### 4. TokenAccessor для передачи токена в API

Создан сервис `ITokenAccessor` для получения и передачи access token:
- `GetAccessTokenAsync()` — получает токен текущего пользователя из HttpContext
- `SetAuthorizationHeaderAsync()` — устанавливает заголовок Authorization для HTTP-клиента

`ApiProductService` обновлён для использования токена при выполнении защищённых операций (POST, PUT, DELETE).

### 5. Регистрация пользователей

#### Компоненты:
- `RegisterUserViewModel` — модель с валидацией
- `IKeycloakUserService` — сервис для создания пользователей через Keycloak Admin API
- `IFileService` — сервис для сохранения аватаров
- `AccountController.Register` — действие для обработки регистрации

#### Admin API Keycloak:
```
POST /admin/realms/{realm}/users
```

Создание пользователя с атрибутом `avatar` и временным паролем.

### 6. UI компоненты

#### `AccountController`:
- `Login()` — перенаправление на Keycloak для аутентификации
- `Logout()` — выход из системы (Cookie + Keycloak)
- `Register()` — страница и обработка регистрации

#### `_UserInfoPartial.cshtml`:
- Для неавторизованных: кнопки Login/Register
- Для авторизованных: имя пользователя, аватар, Logout

#### Получение claims пользователя:
```csharp
User.Identity.IsAuthenticated
User.Claims.FirstOrDefault(c => c.Type == "preferred_username")
User.Claims.FirstOrDefault(c => c.Type == "avatar")
```

### 7. Ограничение доступа к Razor Pages

```csharp
app.MapRazorPages().RequireAuthorization("admin");
```

Страницы администратора (CRUD для Dish) доступны только пользователям с ролью `POWER-USER`.

## Использованные технологии

- **Keycloak 26.0.4** — сервер аутентификации
- **Microsoft.AspNetCore.Authentication.JwtBearer** — JWT аутентификация для API
- **Microsoft.AspNetCore.Authentication.OpenIdConnect** — OIDC аутентификация для UI
- **Policy-based Authorization** — авторизация на основе ролей
- **HttpClient** с Bearer Token — защищённые запросы к API

## Схема аутентификации

1. Пользователь нажимает Login
2. UI перенаправляет на Keycloak (`/auth/realms/{realm}/protocol/openid-connect/auth`)
3. Пользователь вводит логин/пароль на странице Keycloak
4. Keycloak перенаправляет обратно с authorization code
5. UI обменивает code на access_token и id_token
6. Токены сохраняются в Cookie
7. При запросах к API токен передаётся в заголовке `Authorization: Bearer <token>`
