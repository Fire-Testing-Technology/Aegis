using System.ComponentModel.DataAnnotations;

namespace Aegis.Server.AspNetCore.Entities;

public class ActivityLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    public Guid? ActorUserId { get; set; }

    [StringLength(30)]
    public string? ActorUsername { get; set; }

    [StringLength(64)]
    public string Action { get; set; } = string.Empty;

    [StringLength(32)]
    public string EntityType { get; set; } = string.Empty;

    [StringLength(64)]
    public string? EntityId { get; set; }

    [StringLength(500)]
    public string? Summary { get; set; }

    public bool Succeeded { get; set; } = true;
}
