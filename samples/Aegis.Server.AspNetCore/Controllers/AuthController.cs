using Aegis.Server.AspNetCore.DTOs;
using Aegis.Server.AspNetCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aegis.Server.AspNetCore.Controllers;

[ApiController]
public sealed class AuthController(CookieSignInService signInService, AuthService authService) : ControllerBase
{
    [HttpPost("/auth/login")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Login([FromForm] LoginDto login)
    {
        var ok = await signInService.SignInAsync(HttpContext, login);
        return ok
            ? Redirect("/")
            : Redirect("/login?error=1");
    }

    [HttpPost("/auth/register")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Register([FromForm] RegisterDto registration)
    {
        if (string.IsNullOrWhiteSpace(registration.Username)
            || string.IsNullOrWhiteSpace(registration.Email)
            || string.IsNullOrWhiteSpace(registration.FullName)
            || string.IsNullOrWhiteSpace(registration.Password)
            || string.IsNullOrWhiteSpace(registration.ConfirmPassword))
        {
            return Redirect("/register?error=invalid");
        }

        if (!string.Equals(registration.Password, registration.ConfirmPassword, StringComparison.Ordinal))
            return Redirect("/register?error=mismatch");

        // Public self-registration is limited to the User role; admins are created by an Admin.
        registration.Role = "User";

        var ok = await authService.RegisterAsync(registration);
        return ok
            ? Redirect("/login?registered=1")
            : Redirect("/register?error=taken");
    }

    [HttpGet("/auth/logout")]
    [HttpPost("/auth/logout")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInService.SignOutAsync(HttpContext);
        return Redirect("/login");
    }
}

