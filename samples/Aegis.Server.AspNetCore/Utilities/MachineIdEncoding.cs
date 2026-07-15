using System.Text;

namespace Aegis.Server.AspNetCore.Utilities;

public static class MachineIdEncoding
{
    public static string DecodeRequestCode(string requestCode)
    {
        if (string.IsNullOrWhiteSpace(requestCode))
            throw new ArgumentException("Request code is required.", nameof(requestCode));

        var normalized = requestCode
            .Trim()
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal);

        try
        {
            var machineId = Encoding.UTF8.GetString(Convert.FromBase64String(normalized));
            if (string.IsNullOrWhiteSpace(machineId))
                throw new FormatException("Request code decoded to an empty value.");

            return machineId;
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Request code is not valid base64.", nameof(requestCode), ex);
        }
    }
}
