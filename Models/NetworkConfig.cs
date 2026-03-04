namespace IPTool.Models;

/// <summary>
/// Represents network configuration for a specific Wi-Fi network.
/// </summary>
public class NetworkConfig
{
    /// <summary>
    /// Gets or sets the static IP address.
    /// </summary>
    public string IPAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subnet mask.
    /// </summary>
    public string SubnetMask { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default gateway.
    /// </summary>
    public string Gateway { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the DNS server.
    /// </summary>
    public string DNSServer { get; set; } = string.Empty;

    /// <summary>
    /// Validates the network configuration.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public bool IsValid()
    {
        return IsValidIPAddress(IPAddress) &&
               IsValidIPAddress(SubnetMask) &&
               IsValidIPAddress(Gateway) &&
               IsValidIPAddress(DNSServer);
    }

    /// <summary>
    /// Validates if a string is a valid IP address.
    /// </summary>
    private static bool IsValidIPAddress(string ipAddress)
    {
        return System.Net.IPAddress.TryParse(ipAddress, out _);
    }
}
