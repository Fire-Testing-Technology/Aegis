using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aegis.Server.AspNetCore.Utilities;

public sealed class LicenseRequestCode
{
    public required string MachineId { get; init; }

    public string? SoftwareUrn { get; init; }
}

public static class MachineIdEncoding
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string DecodeRequestCode(string requestCode) =>
        DecodeRequestCodePayload(requestCode).MachineId;

    public static LicenseRequestCode DecodeRequestCodePayload(string requestCode)
    {
        if (string.IsNullOrWhiteSpace(requestCode))
            throw new ArgumentException("Request code is required.", nameof(requestCode));

        var normalized = requestCode
            .Trim()
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal);

        string decoded;
        try
        {
            decoded = Encoding.UTF8.GetString(Convert.FromBase64String(normalized));
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Request code is not valid base64.", nameof(requestCode), ex);
        }

        if (string.IsNullOrWhiteSpace(decoded))
            throw new ArgumentException("Request code decoded to an empty value.", nameof(requestCode));

        var trimmed = decoded.Trim();
        if (trimmed.StartsWith('{'))
        {
            PayloadDto? payload;
            try
            {
                payload = JsonSerializer.Deserialize<PayloadDto>(trimmed, JsonOptions);
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Request code JSON payload is invalid.", nameof(requestCode), ex);
            }

            if (payload is null || string.IsNullOrWhiteSpace(payload.MachineId))
                throw new ArgumentException("Request code payload is missing machineId.", nameof(requestCode));

            string? urn = null;
            if (!string.IsNullOrWhiteSpace(payload.SoftwareUrn))
            {
                urn = SoftwareUrn.Normalize(payload.SoftwareUrn);
                if (!SoftwareUrn.IsValid(urn))
                    throw new ArgumentException($"Request code software URN is invalid: '{payload.SoftwareUrn}'.", nameof(requestCode));
            }

            return new LicenseRequestCode
            {
                MachineId = payload.MachineId.Trim(),
                SoftwareUrn = urn
            };
        }

        return new LicenseRequestCode { MachineId = trimmed };
    }

    private sealed class PayloadDto
    {
        public int V { get; set; }
        public string? SoftwareUrn { get; set; }
        public string MachineId { get; set; } = string.Empty;
    }
}
