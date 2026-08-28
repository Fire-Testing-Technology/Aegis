namespace Aegis.Server.AspNetCore;

/// <summary>
/// Windows service identity shared by the host and install scripts.
/// </summary>
public static class WindowsServiceInfo
{
    /// <summary>SCM service name (no spaces).</summary>
    public const string ServiceName = "AegisLicensingServer";

    /// <summary>Display name in services.msc.</summary>
    public const string DisplayName = "Aegis Licencing Server";
}
