using IPTool.Models;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace IPTool.Services;

/// <summary>
/// Linux-specific network service implementation.
/// </summary>
public class LinuxNetworkService : IPlatformNetworkService
{
    public string PlatformName => "Linux";

    /// <summary>
    /// Gets the current Wi-Fi SSID using iwgetid or nmcli.
    /// </summary>
    public string? GetCurrentWifiSSID()
    {
        // Try iwgetid first
        var ssid = TryGetSSIDWithIwgetid();
        if (!string.IsNullOrEmpty(ssid))
        {
            return ssid;
        }

        // Fallback to nmcli
        ssid = TryGetSSIDWithNmcli();
        if (!string.IsNullOrEmpty(ssid))
        {
            return ssid;
        }

        throw new InvalidOperationException("Failed to detect Wi-Fi SSID. Please ensure you are connected to a Wi-Fi network.");
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

            return wifiAdapter?.Name;
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
            // Try using nmcli first (NetworkManager)
            var nmcliResult = TrySetStaticIPWithNmcli(adapterName, config);
            if (nmcliResult)
            {
                return true;
            }

            // Fallback to ip command
            return TrySetStaticIPWithIpCommand(adapterName, config);
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
            // Try using nmcli first (NetworkManager)
            var nmcliResult = TrySetDHCPWithNmcli(adapterName);
            if (nmcliResult)
            {
                return true;
            }

            // Fallback to dhclient
            return TrySetDHCPWithDhclient(adapterName);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to set DHCP: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Tries to get SSID using iwgetid command.
    /// </summary>
    private static string? TryGetSSIDWithIwgetid()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "iwgetid",
                Arguments = "-r",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                return null;
            }

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            return process.ExitCode == 0 && !string.IsNullOrEmpty(output) ? output : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Tries to get SSID using nmcli command.
    /// </summary>
    private static string? TryGetSSIDWithNmcli()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "nmcli",
                Arguments = "-t -f active,ssid dev wifi",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                return null;
            }

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                return null;
            }

            // Parse output: format is "yes:SSID" for active connections
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith("yes:"))
                {
                    var ssid = line.Substring(4);
                    return !string.IsNullOrEmpty(ssid) ? ssid : null;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Tries to set static IP using nmcli (NetworkManager).
    /// </summary>
    private static bool TrySetStaticIPWithNmcli(string adapterName, NetworkConfig config)
    {
        try
        {
            // Get the connection name for this adapter
            var connectionName = GetConnectionNameForAdapter(adapterName);
            if (string.IsNullOrEmpty(connectionName))
            {
                return false;
            }

            // Set static IP configuration
            var commands = new[]
            {
                $"connection modify \"{connectionName}\" ipv4.addresses {config.IPAddress}/24",
                $"connection modify \"{connectionName}\" ipv4.gateway {config.Gateway}",
                $"connection modify \"{connectionName}\" ipv4.dns \"{config.DNSServer}\"",
                $"connection modify \"{connectionName}\" ipv4.method manual",
                $"connection up \"{connectionName}\""
            };

            foreach (var cmd in commands)
            {
                var result = ExecuteCommand("nmcli", cmd);
                if (!result)
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Tries to set static IP using ip command.
    /// </summary>
    private static bool TrySetStaticIPWithIpCommand(string adapterName, NetworkConfig config)
    {
        try
        {
            // Flush existing IP addresses
            var flushResult = ExecuteCommand("ip", $"addr flush dev {adapterName}");
            if (!flushResult)
            {
                return false;
            }

            // Add new IP address
            var addIpResult = ExecuteCommand("ip", $"addr add {config.IPAddress}/{config.SubnetMask} dev {adapterName}");
            if (!addIpResult)
            {
                return false;
            }

            // Add default route
            var routeResult = ExecuteCommand("ip", $"route add default via {config.Gateway} dev {adapterName}");
            if (!routeResult)
            {
                return false;
            }

            // Update DNS server
            var dnsResult = UpdateDnsServer(config.DNSServer);
            if (!dnsResult)
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Tries to set DHCP using nmcli (NetworkManager).
    /// </summary>
    private static bool TrySetDHCPWithNmcli(string adapterName)
    {
        try
        {
            // Get the connection name for this adapter
            var connectionName = GetConnectionNameForAdapter(adapterName);
            if (string.IsNullOrEmpty(connectionName))
            {
                return false;
            }

            // Set DHCP
            var commands = new[]
            {
                $"connection modify \"{connectionName}\" ipv4.method auto",
                $"connection up \"{connectionName}\""
            };

            foreach (var cmd in commands)
            {
                var result = ExecuteCommand("nmcli", cmd);
                if (!result)
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Tries to set DHCP using dhclient.
    /// </summary>
    private static bool TrySetDHCPWithDhclient(string adapterName)
    {
        try
        {
            // Release existing DHCP lease
            var releaseResult = ExecuteCommand("dhclient", $"-r {adapterName}");

            // Request new DHCP lease
            var requestResult = ExecuteCommand("dhclient", adapterName);

            return requestResult;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the NetworkManager connection name for a specific adapter.
    /// </summary>
    private static string? GetConnectionNameForAdapter(string adapterName)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "nmcli",
                Arguments = "-t -f device,connection device",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                return null;
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                return null;
            }

            // Parse output: format is "device:connection"
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length >= 2 && parts[0] == adapterName)
                {
                    return parts[1];
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Updates the DNS server by modifying /etc/resolv.conf.
    /// </summary>
    private static bool UpdateDnsServer(string dnsServer)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "/bin/sh",
                Arguments = $"-c \"echo 'nameserver {dnsServer}' | tee /etc/resolv.conf\"",
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

            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Executes a command and returns whether it was successful.
    /// </summary>
    private static bool ExecuteCommand(string fileName, string arguments)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = fileName,
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

            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
