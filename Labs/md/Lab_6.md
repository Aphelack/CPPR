
# Кафедра информатики. Гламаздин И.И.  
# **Лабораторная работа №6**  
## **Аутентификация и авторизация**



## 1. Цель работы
Знакомство с механизмом аутентификации и авторизации в ASP.NET Core.

---

## 2. Задача работы
Научиться выполнять аутентификацию и авторизацию на удаленном сервере.  
Научиться ограничивать доступ к API для незарегистрированных пользователей.  

**Время выполнения работы: 6 часов (3 занятия).**

---

## 3. Предварительная информация

### 3.1. Выбор сервера аутентификации
Для лабораторной необходимо использовать отдельное приложение — **сервер аутентификации Keycloak**.  

Сервер предоставляет:

- страницу входа,
- возможность регистрации,
- выдачу токена,
- проверку токена.

Из-за возможных блокировок внешних сервисов (Azure AD, Google, Microsoft) используется локальное решение — **Keycloak**.

Подробнее: https://www.keycloak.org/

Общая схема работы:

```

Клиент → API-сервис → Сервер аутентификации

````

---

## 3.2. Некоторые термины Keycloak

### 3.2.1. Realm
Realm — домен безопасности. Содержит пользователей, приложения, роли и политики доступа.

### 3.2.2. API scope
Scope — область доступа OAuth2, которую приложение запрашивает у сервера.

Пример:

```csharp
public static IEnumerable<ApiScope> ApiScopes =>
    new ApiScope[]
    {
        new ApiScope("scope1"),
        new ApiScope("scope2"),
    };
````

### 3.2.3. Clients

Client — приложение, которое использует Keycloak:

* веб сайт,
* мобильное приложение,
* сервис,
* API.

---

# 4. Выполнение работы

---

## 4.1. Предварительная информация

Keycloak можно развернуть:

* локально через ZIP-дистрибутив,
* в Docker (рекомендуется).

Используется база данных **PostgreSQL**.

---

## 4.2. Локальная установка Keycloak и PostgreSQL

### 4.2.1. Установка Keycloak

Документация: [https://www.keycloak.org/guides](https://www.keycloak.org/guides)
Установка ZIP-версии: [https://www.keycloak.org/getting-started/getting-started-zip](https://www.keycloak.org/getting-started/getting-started-zip)

Необходимо установить OpenJDK.
После установки назначить SSL-сертификат.

### 4.2.2. Подключение PostgreSQL

Установить PostgreSQL:
[https://www.postgresql.org/download/](https://www.postgresql.org/download/)

Создать:

* базу данных (`keycloak`),
* пользователя (`keycloak`) с полными правами.

В Keycloak:

* обновить конфигурацию в папке `conf`,
* создать файл запуска:

```bat
start bin\kc.bat start-dev
```

---

## 4.3. Запуск Keycloak и PostgreSQL в Docker

Команда остановки контейнеров:

```bash
docker compose down
```

Фрагмент `compose.yaml`:

```yaml
keycloak:
  image: quay.io/keycloak/keycloak:26.0.4
  container_name: keycloak
  command: start-dev
  environment:
    KC_DB: postgres
    KC_DB_URL_HOST: postgres
    KC_DB_URL_PORT: 5432
    KC_DB_USERNAME: ${POSTGRES_USER}
    KC_DB_PASSWORD: ${POSTGRES_PASSWORD}
    KEYCLOAK_ADMIN: ${KEYCLOAK_ADMIN}
    KEYCLOAK_ADMIN_PASSWORD: ${KEYCLOAK_ADMIN_PASSWORD}
  ports:
    - "8080:8080"
  networks:
    - postgres_network
  depends_on:
    - postgres
  restart: unless-stopped
```

Файл `.env`:

```
POSTGRES_USER=admin
POSTGRES_PASSWORD=123456
POSTGRES_DB=keycloak
PGADMIN_DEFAULT_EMAIL=admin@example.com
PGADMIN_DEFAULT_PASSWORD=123456
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=123456
```

Запуск контейнеров:

```bash
docker compose --env-file xxx.env up -d
```

---

## 4.4. Настройка Keycloak

### 4.4.1. Создание Realm

1. Перейдите на `http://localhost:8080`.
2. Создайте новый Realm — **название = ваша фамилия**.
3. Настройки:

   * Email в качестве username.
   * Удалить поля имени и фамилии.
   * Добавить пользовательский атрибут `avatar`.

### 4.4.2. Создание Client

Тип клиента: **OpenID Connect**.

Требуемые настройки:

* Client ID: `[Фамилия]UiClient`
* Включить:

  * Client Authentication
  * Standard Flow (code)
  * Implicit Flow
* Настроить Redirect URI.
* Найти и сохранить **Client Secret**.
* В разделе “Service Account roles” назначить роль `manage-users`.

### 4.4.3. Передача avatar в токене

Добавить mapper:

* Client Scopes → client-dedicated → Create mapper
* Тип: User Attribute
* Имя: avatar

### 4.4.4. Создание роли POWER-USER

В разделе Roles → Add Role.

### 4.4.5. Передача роли в токене

Client Scopes → roles → Mappers → Add mapper:

* Multivalued = ON
* Token Claim Name = role
* Add to access token = ON

### 4.4.6. Создание пользователей

Создать:

* обычного пользователя,
* администратора (назначить роль POWER-USER).

---

## 4.5. Проверка Keycloak через Postman / Insomnia

Получить токен пользователя:

```
POST /realms/{realm}/protocol/openid-connect/token
```

Form-data:

* grant_type=password
* client_id
* client_secret
* username
* password

Получить токен клиента:

* grant_type=client_credentials

---

# 4.6. Настройка проекта XXX.API

## 4.6.1. Добавить NuGet:

```
Microsoft.AspNetCore.Authentication.JwtBearer
```

## 4.6.2. Добавить в appsettings.json:

```json
"AuthServer": {
  "Host": "http://localhost:8080",
  "Realm": "ВашRealm"
}
```

## 4.6.3. Модель конфигурации

```csharp
internal class AuthServerData
{
    public string Host { get; set; }
    public string Realm { get; set; }
}
```

## 4.6.4. Добавление аутентификации

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.MetadataAddress =
        $"{authServer.Host}/realms/{authServer.Realm}/.well-known/openid-configuration";

    options.Authority = $"{authServer.Host}/realms/{authServer.Realm}";
    options.Audience = "account";
    options.RequireHttpsMetadata = false;
});
```

## 4.6.5. Политики авторизации

```csharp
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("admin", p => p.RequireRole("POWER-USER"));
});
```

## 4.6.6. Middleware

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

## 4.6.7. Ограничение доступа к API

```csharp
routes.MapGroup("/api/Dish")
    .RequireAuthorization("admin");
```

Разрешить GET:

```csharp
.MapGet("/", ...).AllowAnonymous();
```

---

# 4.7. Настройка проекта XXX.UI

## 4.7.1. Установить NuGet

```
Microsoft.AspNetCore.Authentication.OpenIdConnect
Microsoft.AspNetCore.Authentication.JwtBearer
```

## 4.7.2. appsettings.json

```json
"Keycloak": {
  "Host": "http://localhost:8080",
  "Realm": "ВашRealm",
  "ClientId": "ВашClientId",
  "ClientSecret": "ВашSecret"
}
```

## 4.7.3. Настройка OpenID Connect

```csharp
builder.Services.AddAuthentication(...)
    .AddOpenIdConnect("keycloak", options =>
    {
        options.Authority = $"{data.Host}/realms/{data.Realm}";
        options.ClientId = data.ClientId;
        options.ClientSecret = data.ClientSecret;
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.RequireHttpsMetadata = false;
    });
```

## 4.7.4. Политика авторизации

```csharp
opt.AddPolicy("admin", p => p.RequireRole("POWER-USER"));
```

## 4.7.5. Ограничение Razor Pages

```csharp
app.MapRazorPages().RequireAuthorization("admin");
```

---

## 4.7.6. Получение токена в UI

Интерфейс:

```csharp
public interface ITokenAccessor
{
    Task SetAuthorizationHeaderAsync(HttpClient httpClient, bool isClient);
}
```

Использование:

```csharp
await _tokenAccessor.SetAuthorizationHeaderAsync(_httpClient, false);
```

---

# 4.8. Реализация регистрации на сервере аутентификации

### Требования

* сохранить аватар в `wwwroot/Images`
* имя файла — случайное
* путь сохранить в пользовательский атрибут `Avatar`
* если файл не загружен — использовать изображение по умолчанию

---

## 4.8.1. Интерфейс сервиса файлов

```csharp
public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file);
}
```

---

## 4.8.2. Модель регистрации

```csharp
class RegisterUserViewModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public IFormFile? Avatar { get; set; }
}
```

## 4.8.3. Создание пользователя через Admin API

Метод:

```
POST /admin/realms/{realm}/users
```

---

## 4.8.4. Представление Register

Требования:

* форма должна иметь `enctype="multipart/form-data"`
* добавить поле `input type="file"`
* использовать Bootstrap

---

# 4.9. Доработка XXX.UI

### Если пользователь не авторизован:

* показывать Login
* показывать Register

### Если авторизован:

* показывать:

  * корзину
  * имя пользователя
  * аватар

---

## Методы Login и Logout

```csharp
public async Task Login()
{
    await HttpContext.ChallengeAsync("keycloak",
        new AuthenticationProperties
        {
            RedirectUri = Url.Action("Index", "Home")
        });
}

[HttpPost]
public async Task Logout()
{
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await HttpContext.SignOutAsync("keycloak",
        new AuthenticationProperties
        {
            RedirectUri = Url.Action("Index", "Home")
        });
}
```

---

## Частичное представление _UserInfoPartial

```csharp
User.Identity.IsAuthenticated
```

Получение полей:

```csharp
User.Claims.FirstOrDefault(c => c.Type == "preferred_username")
User.Claims.FirstOrDefault(c => c.Type == "avatar")
```

## Контрольные вопросы
1. Какой механизм аутентификации имеет встроенную поддержку в
ASP.Net Core?
2. Что описывают классы ClaimsPrincipal и ClaimsIdentity?
3. Как подключить Middleware аутентификации и авторизации?
4. Приведите пример использования свойства HttpContext.User.
5. Как в коде проверить, что пользователь прошел аутентификацию?
6. Как получить значение Claim пользователя?
7. Как получить Id пользователя, прошедшего аутентификацию?
8. Как разрешить доступ к контроллеру только для пользователей с
ролью «manager»?
9. Как создать политику авторизации с помощью Claim?
10. Как создать куки аутентификации с помощью объекта HttpContext?
11. Как добавить в проект использование системы членства
Microsoft.AspnetCore.Identity?
12. Как с помощью системы членства Microsoft.AspnetCore.Identity
создать нового пользователя?
13. Как с помощью системы членства Microsoft.AspnetCore.Identity
осуществить вход пользователя в систему?
14. Как с помощью системы членства Microsoft.AspnetCore.Identity
добавить Claim пользователю?
15. Какой интерфейс используется в Microsoft.AspnetCore.Identity для
доступа к хранилищу пользователей?