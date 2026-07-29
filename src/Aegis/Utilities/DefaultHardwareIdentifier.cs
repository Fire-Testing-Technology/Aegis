using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Aegis.Interfaces;
using Microsoft.Win32;

namespace Aegis.Utilities;

public class DefaultHardwareIdentifier : IHardwareIdentifier
{
    /// <summary>
    ///     Gets a stable hardware identifier for the current machine.
    ///     Uses an OS-provided device id where available (Windows MachineGuid, Linux machine-id, macOS platform UUID),
    ///     falling back to a sorted physical NIC fingerprint.
    /// </summary>
    public string GetHardwareIdentifier()
    {
        var deviceId = GetStableDeviceId() ?? GetSortedPhysicalMacFingerprint();
        return $"{Environment.MachineName}-{deviceId}";
    }

    /// <summary>
    ///     Validates a hardware identifier against the current machine's hardware identifier.
    /// </summary>
    public bool ValidateHardwareIdentifier(string hardwareId) =>
        string.Equals(GetHardwareIdentifier(), hardwareId, StringComparison.Ordinal);

    private static string? GetStableDeviceId()
    {
        if (OperatingSystem.IsWindows())
            return TryGetWindowsMachineGuid();

        if (OperatingSystem.IsLinux())
            return TryReadMachineIdFile("/etc/machine-id")
                   ?? TryReadMachineIdFile("/var/lib/dbus/machine-id");

        if (OperatingSystem.IsMacOS())
            return TryGetMacOsPlatformUuid();

        return null;
    }

    private static string? TryGetWindowsMachineGuid()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
            var machineGuid = key?.GetValue("MachineGuid") as string;
            return string.IsNullOrWhiteSpace(machineGuid) ? null : machineGuid.Trim();
        }
        catch
        {
            return null;
        }
    }

    private static string? TryReadMachineIdFile(string path)
    {
        try
        {
            if (!File.Exists(path))
                return null;

            var machineId = File.ReadAllText(path).Trim();
            return string.IsNullOrWhiteSpace(machineId) ? null : machineId;
        }
        catch
        {
            return null;
        }
    }

    private static string? TryGetMacOsPlatformUuid()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "ioreg",
                Arguments = "-rd1 -c IOPlatformExpertDevice",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (process is null)
                return null;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
                return null;

            var match = Regex.Match(output, "\"IOPlatformUUID\"\\s*=\\s*\"([^\"]+)\"");
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }
        catch
        {
            return null;
        }
    }

    private static string GetSortedPhysicalMacFingerprint()
    {
        var values = NetworkInterface.GetAllNetworkInterfaces()
            .Where(x => x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .Where(x => x.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
            .Where(x => !x.Description.Contains("virtual", StringComparison.OrdinalIgnoreCase))
            .Where(x => !x.Description.Contains("vpn", StringComparison.OrdinalIgnoreCase))
            .Where(x => !x.Description.Contains("hyper-v", StringComparison.OrdinalIgnoreCase))
            .Where(x => x.Name != "docker0")
            .Select(x => FormatMacAddress(x.GetPhysicalAddress().ToString()))
            .Where(x => !string.IsNullOrWhiteSpace(x) && x != "00:00:00:00:00:00")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return values.Length > 0
            ? string.Join(",", values)
            : "000000000000";
    }

    private static string? FormatMacAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "000000000000")
            return null;

        if (value.Length is not (12 or 16))
            return value;

        var parts = Enumerable.Range(0, value.Length / 2)
            .Select(i => value.Substring(i * 2, 2));
        return string.Join(":", parts);
    }
}
