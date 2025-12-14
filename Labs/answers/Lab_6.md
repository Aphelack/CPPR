# Ответы на вопросы для самопроверки (Лабораторная работа №6)

### 1. Какой механизм аутентификации имеет встроенную поддержку в ASP.Net Core?
ASP.NET Core поддерживает аутентификацию на основе Cookies, JWT Bearer токенов, OAuth 2.0, OpenID Connect, а также систему ASP.NET Core Identity.

### 2. Что описывают классы ClaimsPrincipal и ClaimsIdentity?
- **ClaimsIdentity**: Представляет собой удостоверение пользователя (как паспорт), содержащее набор утверждений (Claims).
- **ClaimsPrincipal**: Представляет самого пользователя, который может иметь несколько удостоверений (например, паспорт и водительские права). Это основной объект `User` в `HttpContext`.

### 3. Как подключить Middleware аутентификации и авторизации?
В `Program.cs` вызвать `app.UseAuthentication()` и `app.UseAuthorization()`. Важно соблюдать порядок: сначала Authentication, потом Authorization.

### 4. Приведите пример использования свойства HttpContext.User.
```csharp
var userName = HttpContext.User.Identity.Name;
var isAuthenticated = HttpContext.User.Identity.IsAuthenticated;
```

### 5. Как в коде проверить, что пользователь прошел аутентификацию?
Проверить свойство `User.Identity.IsAuthenticated`.

### 6. Как получить значение Claim пользователя?
```csharp
var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
// или
var email = User.FindFirst(ClaimTypes.Email)?.Value;
```

### 7. Как получить Id пользователя, прошедшего аутентификацию?
Обычно Id хранится в Claim с типом `ClaimTypes.NameIdentifier` (или `sub` в JWT).
```csharp
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
```

### 8. Как разрешить доступ к контроллеру только для пользователей с ролью «manager»?
Использовать атрибут `[Authorize(Roles = "manager")]`.

### 9. Как создать политику авторизации с помощью Claim?
В `Program.cs`:
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmployeeOnly", policy => policy.RequireClaim("Department", "Sales"));
});
```

### 10. Как создать куки аутентификации с помощью объекта HttpContext?
Используя `HttpContext.SignInAsync`:
```csharp
await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
```

### 11. Как добавить в проект использование системы членства Microsoft.AspnetCore.Identity?
1. Установить пакет `Microsoft.AspNetCore.Identity.EntityFrameworkCore`.
2. Унаследовать контекст БД от `IdentityDbContext`.
3. В `Program.cs` вызвать `builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<AppDbContext>();`.

### 12. Как с помощью системы членства Microsoft.AspnetCore.Identity создать нового пользователя?
Используя `UserManager<TUser>`:
```csharp
var result = await _userManager.CreateAsync(user, password);
```

### 13. Как с помощью системы членства Microsoft.AspnetCore.Identity осуществить вход пользователя в систему?
Используя `SignInManager<TUser>`:
```csharp
var result = await _signInManager.PasswordSignInAsync(email, password, isPersistent, lockoutOnFailure);
```

### 14. Как с помощью системы членства Microsoft.AspnetCore.Identity добавить Claim пользователю?
```csharp
await _userManager.AddClaimAsync(user, new Claim("type", "value"));
```

### 15. Какой интерфейс используется в Microsoft.AspnetCore.Identity для доступа к хранилищу пользователей?
`IUserStore<TUser>`.
