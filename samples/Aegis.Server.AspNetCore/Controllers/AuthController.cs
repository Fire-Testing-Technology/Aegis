using Aegis.Server.AspNetCore.DTOs;
using Aegis.Server.AspNetCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aegis.Server.AspNetCore.Controllers;

[ApiController]
public sealed class AuthController(CookieSignInService signInService) : ControllerBase
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

    [HttpPost("/auth/logout")]
    [Authorize]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInService.SignOutAsync(HttpContext);
        return Redirect("/login");
    }
}

