using System.Security.Claims;
using Aegis.Server.AspNetCore.DTOs;
using Aegis.Server.AspNetCore.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Aegis.Server.AspNetCore.Services;

public class CookieSignInService(AuthService authService, ActivityLogService activityLog)
{
	public async Task<bool> SignInAsync(HttpContext httpContext, LoginDto login)
	{
		var attemptedUser = string.IsNullOrWhiteSpace(login.Username) ? "(empty)" : login.Username;
		var user = await authService.ValidateCredentialsAsync(login);
		if (user == null)
		{
			await activityLog.LogAsync(
				ActivityActions.Login,
				"Auth",
				entityId: attemptedUser,
				summary: $"Login failed for '{attemptedUser}'.",
				succeeded: false,
				actorUsername: attemptedUser);
			return false;
		}

		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, user.Id.ToString()),
			new(ClaimTypes.Name, user.Username),
			new(ClaimTypes.Role, user.Role),
			new(ClaimTypes.Email, user.Email)
		};

		var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
		await httpContext.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			new ClaimsPrincipal(identity),
			new AuthenticationProperties
			{
				IsPersistent = true,
				ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
			});

		await activityLog.LogAsync(
			ActivityActions.Login,
			"Auth",
			entityId: user.Id.ToString(),
			summary: $"User '{user.Username}' signed in.",
			succeeded: true,
			actorUserId: user.Id,
			actorUsername: user.Username);

		return true;
	}

	public async Task SignOutAsync(HttpContext httpContext)
	{
		var principal = httpContext.User;
		Guid? actorId = null;
		var idValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
		if (Guid.TryParse(idValue, out var id))
			actorId = id;
		var username = principal.Identity?.Name
			?? principal.FindFirstValue(ClaimTypes.Name);

		await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

		await activityLog.LogAsync(
			ActivityActions.Logout,
			"Auth",
			entityId: actorId?.ToString() ?? username,
			summary: string.IsNullOrWhiteSpace(username)
				? "User signed out."
				: $"User '{username}' signed out.",
			succeeded: true,
			actorUserId: actorId,
			actorUsername: username);
	}
}
