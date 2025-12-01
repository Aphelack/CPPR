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

    public async Task Login(string? returnUrl = null)
    {
        await HttpContext.ChallengeAsync("keycloak", new AuthenticationProperties
        {
            RedirectUri = returnUrl ?? Url.Action("Index", "Home")
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync("keycloak", new AuthenticationProperties
        {
            RedirectUri = Url.Action("Index", "Home")
        });
        
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
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
