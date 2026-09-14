using System.Text.Json.Serialization;
using Aegis.Enums;

namespace Aegis.Models.License;

[JsonDerivedType(typeof(TrialLicense), "Trial")]
public class TrialLicense : BaseLicense
{
    [JsonConstructor]
    protected TrialLicense()
    {
        Type = LicenseType.Trial;
    }

    public TrialLicense(TimeSpan trialPeriod)
    {
        TrialPeriod = trialPeriod;
        // Expiry is computed on the client from first activation + TrialPeriod.
        ExpirationDate = null;
        Type = LicenseType.Trial;
    }

    public TrialLicense(BaseLicense license, TimeSpan trialPeriod, string? issuedTo = null)
    {
        TrialPeriod = trialPeriod;
        ExpirationDate = null;
        Type = LicenseType.Trial;
        Features = license.Features;
        Issuer = license.Issuer;
        SoftwareUrn = license.SoftwareUrn;
        LicenseId = license.LicenseId;
        LicenseKey = license.LicenseKey;
        Type = license.Type;
        IssuedOn = license.IssuedOn;
        IssuedTo = issuedTo ?? string.Empty;
    }

    [JsonInclude] public TimeSpan TrialPeriod { get; protected init; }

    [JsonInclude] public string IssuedTo { get; protected internal set; } = string.Empty;
}
