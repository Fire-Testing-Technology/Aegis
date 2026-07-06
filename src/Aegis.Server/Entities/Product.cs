namespace Aegis.Server.Entities;

public class Product
{
    public Guid ProductId { get; set; } = Guid.NewGuid();
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Stable public identifier, e.g. urn:ftt:software:conecalc:7
    /// </summary>
    public string SoftwareUrn { get; set; } = string.Empty;

    // Navigation property
    public ICollection<License> Licenses { get; set; } = [];
    public ICollection<LicenseFeature> LicenseFeatures { get; set; } = [];
    public ICollection<Feature> Features { get; set; } = [];
}