namespace Aegis.Server.AspNetCore;

/// <summary>
/// Resolves durable data paths for the Aegis server when running as a Windows service or in Production.
/// </summary>
public static class ServicePaths
{
    public const string CompanyFolderName = "Fire Testing Technology";
    public const string ProductFolderName = "Aegis";

    /// <summary>
    /// %ProgramData%\Fire Testing Technology\Aegis
    /// </summary>
    public static string DataDirectory
    {
        get
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                CompanyFolderName,
                ProductFolderName);
            Directory.CreateDirectory(path);
            return path;
        }
    }

    public static string DefaultDatabasePath => Path.Combine(DataDirectory, "aegis.db");

    public static string DefaultSecretsPath => Path.Combine(DataDirectory, "aegis-signature.bin");

    public static string LogsDirectory
    {
        get
        {
            var path = Path.Combine(DataDirectory, "logs");
            Directory.CreateDirectory(path);
            return path;
        }
    }

    /// <summary>
    /// Development: next to the exe. Production/service: ProgramData (survives reinstalls).
    /// </summary>
    public static string ResolveSecretsPath(bool useProgramData) =>
        useProgramData
            ? DefaultSecretsPath
            : Path.Combine(AppContext.BaseDirectory, "aegis-signature.bin");

    public static string ResolveSqliteConnectionString(string? configuredConnectionString, bool useProgramData)
    {
        if (string.IsNullOrWhiteSpace(configuredConnectionString))
            return useProgramData
                ? $"Data Source={DefaultDatabasePath}"
                : "Data Source=aegis.db";

        if (!useProgramData)
            return configuredConnectionString;

        const string prefix = "Data Source=";
        var index = configuredConnectionString.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
            return configuredConnectionString;

        var start = index + prefix.Length;
        var remainder = configuredConnectionString[start..];
        var end = remainder.IndexOf(';');
        var dataSource = end >= 0 ? remainder[..end] : remainder;
        dataSource = dataSource.Trim().Trim('"');

        if (Path.IsPathRooted(dataSource))
            return configuredConnectionString;

        var absolute = Path.Combine(DataDirectory, dataSource);
        var suffix = end >= 0 ? remainder[end..] : string.Empty;
        return $"{configuredConnectionString[..index]}{prefix}{absolute}{suffix}";
    }
}
