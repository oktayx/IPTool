using IPTool.Models;
using System.Text.Json;

namespace IPTool.Services;

/// <summary>
/// Manages loading and saving Wi-Fi configuration.
/// </summary>
public class ConfigManager
{
    private const string DefaultConfigFileName = "wifi-config.json";
    private readonly string _configFilePath;

    /// <summary>
    /// Initializes a new instance of the ConfigManager class.
    /// </summary>
    /// <param name="configFilePath">Optional path to the configuration file. If not provided, uses default.</param>
    public ConfigManager(string? configFilePath = null)
    {
        _configFilePath = configFilePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultConfigFileName);
    }

    /// <summary>
    /// Loads the Wi-Fi configuration from file.
    /// </summary>
    /// <returns>The loaded Wi-Fi configuration.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the configuration file is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the configuration file is invalid.</exception>
    public WifiConfig LoadConfig()
    {
        if (!File.Exists(_configFilePath))
        {
            throw new FileNotFoundException(
                $"Configuration file not found: {_configFilePath}. " +
                "Please create a wifi-config.json file with your network configurations.");
        }

        try
        {
            var json = File.ReadAllText(_configFilePath);
            var config = JsonSerializer.Deserialize<WifiConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (config == null)
            {
                throw new InvalidOperationException("Failed to deserialize configuration file.");
            }

            return config;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid JSON in configuration file: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load configuration: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Saves the Wi-Fi configuration to file.
    /// </summary>
    /// <param name="config">The configuration to save.</param>
    public void SaveConfig(WifiConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_configFilePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save configuration: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets the configuration file path.
    /// </summary>
    public string ConfigFilePath => _configFilePath;
}
