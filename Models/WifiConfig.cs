namespace IPTool.Models;

/// <summary>
/// Represents the Wi-Fi configuration containing multiple network configurations.
/// </summary>
public class WifiConfig
{
    /// <summary>
    /// Gets or sets the dictionary of network configurations keyed by Wi-Fi SSID.
    /// </summary>
    public Dictionary<string, NetworkConfig> Networks { get; set; } = new();

    /// <summary>
    /// Gets the network configuration for a specific Wi-Fi SSID.
    /// </summary>
    /// <param name="ssid">The Wi-Fi SSID.</param>
    /// <returns>The network configuration if found, null otherwise.</returns>
    public NetworkConfig? GetConfigForSSID(string ssid)
    {
        return Networks.TryGetValue(ssid, out var config) ? config : null;
    }

    /// <summary>
    /// Adds or updates a network configuration for a specific Wi-Fi SSID.
    /// </summary>
    /// <param name="ssid">The Wi-Fi SSID.</param>
    /// <param name="config">The network configuration.</param>
    public void AddOrUpdateConfig(string ssid, NetworkConfig config)
    {
        Networks[ssid] = config;
    }

    /// <summary>
    /// Removes a network configuration for a specific Wi-Fi SSID.
    /// </summary>
    /// <param name="ssid">The Wi-Fi SSID.</param>
    /// <returns>True if removed, false if not found.</returns>
    public bool RemoveConfig(string ssid)
    {
        return Networks.Remove(ssid);
    }

    /// <summary>
    /// Gets all configured Wi-Fi SSIDs.
    /// </summary>
    /// <returns>A list of configured SSIDs.</returns>
    public List<string> GetAllSSIDs()
    {
        return Networks.Keys.ToList();
    }
}
