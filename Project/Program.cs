using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Project.Extensions;
using DotNetEnv;
using Serilog;
using Project.Middleware;

// Load .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

builder.RegisterCustomServices();

// Keycloak Authentication
var keycloakHost = builder.Configuration["Keycloak:Host"];
var keycloakRealm = builder.Configuration["Keycloak:Realm"];
var keycloakClientId = builder.Configuration["Keycloak:ClientId"];
var keycloakClientSecret = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_SECRET") 
    ?? builder.Configuration["Keycloak:ClientSecret"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "keycloak";
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
})
.AddOpenIdConnect("keycloak", options =>
{
    options.Authority = $"{keycloakHost}/realms/{keycloakRealm}";
    options.ClientId = keycloakClientId;
    options.ClientSecret = keycloakClientSecret;
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.SaveTokens = true;
    options.RequireHttpsMetadata = false;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.GetClaimsFromUserInfoEndpoint = true;
    
    options.TokenValidationParameters.NameClaimType = "preferred_username";
    options.TokenValidationParameters.RoleClaimType = "role";
    
    options.ClaimActions.MapJsonKey("role", "role", "role");
    options.ClaimActions.MapUniqueJsonKey("avatar", "avatar");
    
    options.Events = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProvider = context =>
        {
            // Добавляем prompt=login если указано
            if (context.Properties.Items.TryGetValue("prompt", out var prompt))
            {
                context.ProtocolMessage.Prompt = prompt;
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            // Маппинг ролей из Keycloak realm_access.roles
            var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
            if (identity == null) return Task.CompletedTask;
            
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("OpenIdConnect");
            
            // Получаем access_token из SecurityToken или TokenEndpointResponse
            string? accessToken = null;
            
            if (context.TokenEndpointResponse?.AccessToken != null)
            {
                accessToken = context.TokenEndpointResponse.AccessToken;
            }
            else if (context.SecurityToken is System.IdentityModel.Tokens.Jwt.JwtSecurityToken jwtToken)
            {
                // Используем id_token если access_token недоступен
                accessToken = jwtToken.RawData;
            }
            
            logger.LogInformation("AccessToken available: {Available}", !string.IsNullOrEmpty(accessToken));
            
            if (!string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(accessToken);
                    
                    logger.LogInformation("JWT Claims: {Claims}", string.Join(", ", jwt.Claims.Select(c => $"{c.Type}={c.Value}")));
                    
                    // realm_access.roles
                    var realmAccessClaim = jwt.Claims.FirstOrDefault(c => c.Type == "realm_access");
                    if (realmAccessClaim != null)
                    {
                        logger.LogInformation("realm_access found: {Value}", realmAccessClaim.Value);
                        var realmAccess = System.Text.Json.JsonDocument.Parse(realmAccessClaim.Value);
                        if (realmAccess.RootElement.TryGetProperty("roles", out var roles))
                        {
                            foreach (var role in roles.EnumerateArray())
                            {
                                var roleValue = role.GetString();
                                if (!string.IsNullOrEmpty(roleValue))
                                {
                                    identity.AddClaim(new System.Security.Claims.Claim(
                                        System.Security.Claims.ClaimTypes.Role, roleValue));
                                    logger.LogInformation("Role added: {Role}", roleValue);
                                }
                            }
                        }
                    }
                    else
                    {
                        logger.LogWarning("realm_access claim not found in token");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error parsing JWT token");
                }
            }
            return Task.CompletedTask;
        },
        OnUserInformationReceived = context =>
        {
            // Debug: выводим все данные из userinfo
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("OpenIdConnect");
            logger.LogInformation("UserInfo response: {UserInfo}", context.User.RootElement.ToString());
            
            // Keycloak возвращает атрибуты как массивы
            if (context.User.RootElement.TryGetProperty("avatar", out var avatarElement))
            {
                string? avatarValue = null;
                if (avatarElement.ValueKind == System.Text.Json.JsonValueKind.Array && avatarElement.GetArrayLength() > 0)
                {
                    avatarValue = avatarElement[0].GetString();
                }
                else if (avatarElement.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    avatarValue = avatarElement.GetString();
                }
                
                if (!string.IsNullOrEmpty(avatarValue))
                {
                    var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                    identity?.AddClaim(new System.Security.Claims.Claim("avatar", avatarValue));
                    logger.LogInformation("Avatar claim added: {Avatar}", avatarValue);
                }
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("admin", p => p.RequireAssertion(context =>
        context.User.HasClaim(c => 
            (c.Type == System.Security.Claims.ClaimTypes.Role || c.Type == "role") 
            && c.Value == "POWER-USER")));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseMiddleware<LogMiddleware>();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages().RequireAuthorization("admin");

app.Run();
