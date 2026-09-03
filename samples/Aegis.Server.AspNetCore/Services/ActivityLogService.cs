using System.Security.Claims;
using Aegis.Server.AspNetCore.Data.Context;
using Aegis.Server.AspNetCore.Entities;

namespace Aegis.Server.AspNetCore.Services;

public sealed class ActivityLogService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
{
    public Task LogAsync(
        string action,
        string entityType,
        string? entityId = null,
        string? summary = null,
        bool succeeded = true,
        Guid? actorUserId = null,
        string? actorUsername = null)
    {
        ResolveActor(ref actorUserId, ref actorUsername);

        db.ActivityLogs.Add(new ActivityLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = Truncate(entityId, 64),
            Summary = Truncate(summary, 500),
            Succeeded = succeeded,
            ActorUserId = actorUserId,
            ActorUsername = Truncate(actorUsername, 30),
            OccurredAtUtc = DateTime.UtcNow
        });

        return db.SaveChangesAsync();
    }

    private void ResolveActor(ref Guid? actorUserId, ref string? actorUsername)
    {
        if (actorUserId is not null && !string.IsNullOrWhiteSpace(actorUsername))
            return;

        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return;

        if (actorUserId is null)
        {
            var idValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(idValue, out var id))
                actorUserId = id;
        }

        actorUsername ??= user.Identity?.Name
            ?? user.FindFirstValue(ClaimTypes.Name);
    }

    private static string? Truncate(string? value, int max) =>
        value is null || value.Length <= max ? value : value[..max];
}
