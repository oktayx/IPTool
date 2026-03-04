using IPTool.Models;
using System.Runtime.InteropServices;

namespace IPTool.Services;

/// <summary>
/// Interface for platform-specific network operations.
/// </summary>
public interface IPlatformNetworkService
{
    /// <summary>
    /// Gets the name of the platform (e.g., "Windows", "Linux").
    /// </summary>
    string PlatformName { get; }

    /// <summary>
    /// Gets the current Wi-Fi SSID.
    /// </summary>
    /// <returns>The current SSID if connected, null otherwise.</returns>
    /// <exception cref="InvalidOperationException">Thrown when unable to detect Wi-Fi SSID.</exception>
    string? GetCurrentWifiSSID();

    /// <summary>
    /// Gets the name of the active Wi-Fi network adapter.
    /// </summary>
    /// <returns>The adapter name if found, null otherwise.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no active Wi-Fi adapter is found.</exception>
    string? GetActiveWifiAdapter();

    /// <summary>
    /// Sets a static IP configuration for the specified network adapter.
    /// </summary>
    /// <param name="adapterName">The name of the network adapter.</param>
    /// <param name="config">The network configuration to apply.</param>
    /// <returns>True if successful, false otherwise.</returns>
    /// <exception cref="InvalidOperationException">Thrown when unable to set static IP.</exception>
    bool SetStaticIP(string adapterName, NetworkConfig config);

    /// <summary>
    /// Sets the network adapter to use DHCP (dynamic IP).
    /// </summary>
    /// <param name="adapterName">The name of the network adapter.</param>
    /// <returns>True if successful, false otherwise.</returns>
    /// <exception cref="InvalidOperationException">Thrown when unable to set DHCP.</exception>
    bool SetDHCP(string adapterName);
}
