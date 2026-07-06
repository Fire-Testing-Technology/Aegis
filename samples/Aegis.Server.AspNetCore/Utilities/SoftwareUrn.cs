using System.Text.RegularExpressions;

namespace Aegis.Server.AspNetCore.Utilities;

public static partial class SoftwareUrn
{
    public const string Prefix = "urn:ftt:software:";

    [GeneratedRegex(@"^urn:ftt:software:[a-z0-9-]+:[0-9]+$", RegexOptions.CultureInvariant)]
    private static partial Regex UrnPattern();

    public static string Normalize(string urn) => urn.Trim().ToLowerInvariant();

    public static bool IsValid(string? urn) =>
        !string.IsNullOrWhiteSpace(urn) && UrnPattern().IsMatch(Normalize(urn));

    public static string Format(string slug, int majorVersion) =>
        $"{Prefix}{slug.Trim().ToLowerInvariant()}:{majorVersion}";

    public static string SlugFromName(string name) =>
        new string(name.Trim().ToLowerInvariant()
            .Where(c => char.IsAsciiLetterOrDigit(c) || c == '-')
            .ToArray());

    public static string BuildDisplayName(string name, int majorVersion) =>
        $"{name.Trim()} {majorVersion}";

    public static bool TryCreate(string name, int majorVersion, out string urn)
    {
        urn = string.Empty;
        if (majorVersion < 1 || string.IsNullOrWhiteSpace(name))
            return false;

        var slug = SlugFromName(name);
        if (slug.Length == 0)
            return false;

        urn = Format(slug, majorVersion);
        return IsValid(urn);
    }
}
