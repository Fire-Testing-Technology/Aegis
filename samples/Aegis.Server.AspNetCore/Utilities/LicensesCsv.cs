using System.Globalization;
using System.Text;
using Aegis.Enums;

namespace Aegis.Server.AspNetCore.Utilities;

public static class LicensesCsv
{
    public static readonly string[] ExportHeaders =
    [
        "LicenseKey",
        "Product",
        "SoftwareUrn",
        "Type",
        "IssuedTo",
        "Issuer",
        "Status",
        "IssuedOnUtc",
        "ExpiresUtc"
    ];

    /// <summary>
    /// Optional columns accepted on import (in addition to <see cref="ExportHeaders"/>).
    /// </summary>
    public static readonly string[] ImportExtraHeaders =
    [
        "HardwareId",
        "RequestCode",
        "MaxActiveUsersCount"
    ];

    public sealed class ImportRow
    {
        public int LineNumber { get; init; }
        public string? LicenseKey { get; init; }
        public string? Product { get; init; }
        public string? SoftwareUrn { get; init; }
        public LicenseType Type { get; init; }
        public string IssuedTo { get; init; } = string.Empty;
        public DateTime? IssuedOnUtc { get; init; }
        public DateTime? ExpiresUtc { get; init; }
        public string? HardwareId { get; init; }
        public string? RequestCode { get; init; }
        public int? MaxActiveUsersCount { get; init; }
    }

    public static string Build(IEnumerable<(
        string LicenseKey,
        string Product,
        string SoftwareUrn,
        string Type,
        string IssuedTo,
        string Issuer,
        string Status,
        string IssuedOnUtc,
        string ExpiresUtc)> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(',', ExportHeaders.Select(Quote)));

        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(',',
                Quote(row.LicenseKey),
                Quote(row.Product),
                Quote(row.SoftwareUrn),
                Quote(row.Type),
                Quote(row.IssuedTo),
                Quote(row.Issuer),
                Quote(row.Status),
                Quote(row.IssuedOnUtc),
                Quote(row.ExpiresUtc)));
        }

        return sb.ToString();
    }

    public static List<ImportRow> Parse(string csvText)
    {
        if (string.IsNullOrWhiteSpace(csvText))
            throw new InvalidOperationException("CSV file is empty.");

        var lines = SplitLines(csvText);
        if (lines.Count == 0)
            throw new InvalidOperationException("CSV file is empty.");

        var headerFields = ParseCsvLine(lines[0]);
        var headerIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < headerFields.Count; i++)
        {
            var name = headerFields[i].Trim();
            if (name.Length == 0)
                continue;
            headerIndex[name] = i;
        }

        if (!headerIndex.ContainsKey("Type") || !headerIndex.ContainsKey("IssuedTo"))
            throw new InvalidOperationException("CSV must include Type and IssuedTo columns.");

        if (!headerIndex.ContainsKey("Product") && !headerIndex.ContainsKey("SoftwareUrn"))
            throw new InvalidOperationException("CSV must include Product and/or SoftwareUrn columns.");

        var rows = new List<ImportRow>();
        for (var lineIndex = 1; lineIndex < lines.Count; lineIndex++)
        {
            var line = lines[lineIndex];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = ParseCsvLine(line);
            string? Get(string name) =>
                headerIndex.TryGetValue(name, out var i) && i < fields.Count
                    ? fields[i].Trim()
                    : null;

            var typeText = Get("Type");
            if (string.IsNullOrWhiteSpace(typeText)
                || !Enum.TryParse<LicenseType>(typeText, ignoreCase: true, out var type))
            {
                throw new InvalidOperationException(
                    $"Line {lineIndex + 1}: unknown or missing license Type '{typeText}'.");
            }

            var issuedTo = Get("IssuedTo");
            if (string.IsNullOrWhiteSpace(issuedTo))
                throw new InvalidOperationException($"Line {lineIndex + 1}: IssuedTo is required.");

            DateTime? issuedOn = null;
            var issuedOnText = Get("IssuedOnUtc");
            if (!string.IsNullOrWhiteSpace(issuedOnText))
            {
                if (!DateTime.TryParse(
                        issuedOnText,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces,
                        out var parsedIssuedOn))
                {
                    throw new InvalidOperationException(
                        $"Line {lineIndex + 1}: invalid IssuedOnUtc '{issuedOnText}'.");
                }

                issuedOn = parsedIssuedOn.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(parsedIssuedOn, DateTimeKind.Utc)
                    : parsedIssuedOn.ToUniversalTime();
            }

            DateTime? expires = null;
            var expiresText = Get("ExpiresUtc");
            if (!string.IsNullOrWhiteSpace(expiresText))
            {
                if (!DateTime.TryParse(
                        expiresText,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces,
                        out var parsedExpires))
                {
                    throw new InvalidOperationException(
                        $"Line {lineIndex + 1}: invalid ExpiresUtc '{expiresText}'.");
                }

                expires = parsedExpires.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(parsedExpires, DateTimeKind.Utc)
                    : parsedExpires.ToUniversalTime();
            }

            int? maxUsers = null;
            var maxUsersText = Get("MaxActiveUsersCount");
            if (!string.IsNullOrWhiteSpace(maxUsersText))
            {
                if (!int.TryParse(maxUsersText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedMax)
                    || parsedMax < 1)
                {
                    throw new InvalidOperationException(
                        $"Line {lineIndex + 1}: invalid MaxActiveUsersCount '{maxUsersText}'.");
                }

                maxUsers = parsedMax;
            }

            rows.Add(new ImportRow
            {
                LineNumber = lineIndex + 1,
                LicenseKey = EmptyToNull(Get("LicenseKey")),
                Product = EmptyToNull(Get("Product")),
                SoftwareUrn = EmptyToNull(Get("SoftwareUrn")),
                Type = type,
                IssuedTo = issuedTo,
                IssuedOnUtc = issuedOn,
                ExpiresUtc = expires,
                HardwareId = EmptyToNull(Get("HardwareId")),
                RequestCode = EmptyToNull(Get("RequestCode")),
                MaxActiveUsersCount = maxUsers
            });
        }

        if (rows.Count == 0)
            throw new InvalidOperationException("CSV contains no data rows.");

        return rows;
    }

    public static string Quote(string? value)
    {
        value ??= string.Empty;
        var escaped = value.Replace("\"", "\"\"", StringComparison.Ordinal);
        return $"\"{escaped}\"";
    }

    private static string? EmptyToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static List<string> SplitLines(string text)
    {
        var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        if (normalized.Length > 0 && normalized[0] == '\uFEFF')
            normalized = normalized[1..];

        return normalized.Split('\n', StringSplitOptions.None)
            .Select(l => l.TrimEnd())
            .ToList();
    }

    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            else if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == ',')
            {
                fields.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }

        fields.Add(sb.ToString());
        return fields;
    }
}
