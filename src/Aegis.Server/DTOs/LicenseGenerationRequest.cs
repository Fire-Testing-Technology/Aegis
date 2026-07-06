using Aegis.Enums;
using Aegis.Models;

namespace Aegis.Server.DTOs;

public class LicenseGenerationRequest
{
    public LicenseType LicenseType { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public Guid ProductId { get; set; }
    public string IssuedTo { get; set; } = string.Empty;
    public int? MaxActiveUsersCount { get; set; }
    public string? HardwareId { get; set; }
    public TimeSpan? SubscriptionDuration { get; set; }
    public Guid? IssuingUserId { get; set; }
    public Dictionary<Guid, Feature> Features { get; set; } = [];
}