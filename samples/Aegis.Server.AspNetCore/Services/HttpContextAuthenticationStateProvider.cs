using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace Aegis.Server.AspNetCore.Services;

/// <summary>
/// Reads authentication state from the HTTP request during prerendering and interactive circuits.
/// </summary>
public class HttpContextAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor)
    : ServerAuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
            return Task.FromResult(new AuthenticationState(user));

        return base.GetAuthenticationStateAsync();
    }
}
