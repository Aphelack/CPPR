using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services.Authorization;
using Project.Services.FileService;

namespace Project.Controllers;

public class AccountController : Controller
{
    private readonly IKeycloakUserService _keycloakUserService;
    private readonly IFileService _fileService;

    public AccountController(IKeycloakUserService keycloakUserService, IFileService fileService)
    {
        _keycloakUserService = keycloakUserService;
        _fileService = fileService;
    }

    public IActionResult Login(string? returnUrl = null)
    {
        var redirectUri = returnUrl ?? Url.Action("Index", "Home");
        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUri
        };
        // Принудительно показывать форму входа
        properties.Items["prompt"] = "login";
        
        return Challenge(properties, "keycloak");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Получаем id_token для logout
        var idToken = await HttpContext.GetTokenAsync("id_token");
        
        // Выход из локальной сессии
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        // Выход из Keycloak
        var keycloakHost = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Keycloak:Host"];
        var keycloakRealm = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Keycloak:Realm"];
        var redirectUri = Url.Action("Index", "Home", null, Request.Scheme);
        
        var logoutUrl = $"{keycloakHost}/realms/{keycloakRealm}/protocol/openid-connect/logout" +
                        $"?id_token_hint={idToken}" +
                        $"&post_logout_redirect_uri={Uri.EscapeDataString(redirectUri!)}";
        
        return Redirect(logoutUrl);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string? avatarUrl = null;
        
        if (model.Avatar != null)
        {
            avatarUrl = await _fileService.SaveFileAsync(model.Avatar);
        }

        var (success, error) = await _keycloakUserService.CreateUserAsync(
            model.Email, 
            model.Password, 
            avatarUrl);

        if (success)
        {
            TempData["SuccessMessage"] = "Регистрация успешна! Теперь вы можете войти.";
            return RedirectToAction("Login");
        }

        ModelState.AddModelError(string.Empty, error ?? "Ошибка регистрации");
        return View(model);
    }
}
