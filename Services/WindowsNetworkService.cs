using IPTool.Models;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace IPTool.Services;

/// <summary>
/// Windows-specific network service implementation.
/// </summary>
public class WindowsNetworkService : IPlatformNetworkService
{
    public string PlatformName => "Windows";

    /// <summary>
    /// Gets the current Wi-Fi SSID using netsh command.
    /// </summary>
    public string? GetCurrentWifiSSID()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = "wlan show interfaces",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start netsh process.");
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"netsh command failed with exit code {process.ExitCode}.");
            }

            // Parse the output to find the SSID
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("SSID", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = trimmedLine.Split(':');
                    if (parts.Length >= 2)
                    {
                        var ssid = parts[1].Trim();
                        return !string.IsNullOrEmpty(ssid) ? ssid : null;
                    }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to detect Wi-Fi SSID: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets the active Wi-Fi adapter name.
    /// </summary>
    public string? GetActiveWifiAdapter()
    {
        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            var wifiAdapter = interfaces.FirstOrDefault(ni =>
                ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 &&
                ni.OperationalStatus == OperationalStatus.Up);

            return wifiAdapter?.Description;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get active Wi-Fi adapter: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Sets a static IP configuration for the specified network adapter.
    /// </summary>
    public bool SetStaticIP(string adapterName, NetworkConfig config)
    {
        if (!config.IsValid())
        {
            throw new ArgumentException("Invalid network configuration.", nameof(config));
        }

        try
        {
            // Set static IP address, subnet mask, and gateway
            var setAddressResult = ExecuteNetshCommand(
                $"interface ip set address \"{adapterName}\" static {config.IPAddress} {config.SubnetMask} {config.Gateway}");

            if (!setAddressResult)
            {
                throw new InvalidOperationException("Failed to set IP address.");
            }

            // Set DNS server
            var setDnsResult = ExecuteNetshCommand(
                $"interface ip set dns \"{adapterName}\" static {config.DNSServer}");

            if (!setDnsResult)
            {
                throw new InvalidOperationException("Failed to set DNS server.");
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to set static IP: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Sets the network adapter to use DHCP.
    /// </summary>
    public bool SetDHCP(string adapterName)
    {
        try
        {
            // Set IP to DHCP
            var setAddressResult = ExecuteNetshCommand(
                $"interface ip set address \"{adapterName}\" dhcp");

            if (!setAddressResult)
            {
                throw new InvalidOperationException("Failed to set IP to DHCP.");
            }

            // Set DNS to DHCP
            var setDnsResult = ExecuteNetshCommand(
                $"interface ip set dns \"{adapterName}\" dhcp");

            if (!setDnsResult)
            {
                throw new InvalidOperationException("Failed to set DNS to DHCP.");
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to set DHCP: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Executes a netsh command and returns whether it was successful.
    /// </summary>
    private static bool ExecuteNetshCommand(string arguments)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                return false;
            }

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
