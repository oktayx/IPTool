using IPTool.Models;
using IPTool.Services;
using System.Runtime.InteropServices;

namespace IPTool;

/// <summary>
/// Wi-Fi IP Configuration Tool
/// Automatically configures IP addresses based on the connected Wi-Fi network.
/// </summary>
class Program
{
    private const string Version = "1.0";

    static async Task Main(string[] args)
    {
        Console.WriteLine($"IPTool v{Version} - Wi-Fi IP Configuration Tool");
        Console.WriteLine();

        try
        {
            // Create platform-specific network service
            var networkService = NetworkServiceFactory.CreateService();
            Console.WriteLine($"Platform: {networkService.PlatformName}");
            Console.WriteLine();

            // Load configuration
            var configManager = new ConfigManager();
            var config = configManager.LoadConfig();
            Console.WriteLine($"Configuration loaded from: {configManager.ConfigFilePath}");
            Console.WriteLine();

            // Detect current Wi-Fi network
            Console.WriteLine("Detecting Wi-Fi network...");
            var currentSSID = networkService.GetCurrentWifiSSID();

            if (string.IsNullOrEmpty(currentSSID))
            {
                throw new InvalidOperationException(
                    "No Wi-Fi network detected. Please ensure you are connected to a Wi-Fi network.");
            }

            Console.WriteLine($"Connected to: {currentSSID}");
            Console.WriteLine();

            // Check if the network is in the configuration
            var networkConfig = config.GetConfigForSSID(currentSSID);

            if (networkConfig != null)
            {
                // Apply static IP configuration
                Console.WriteLine("Applying static IP configuration:");
                Console.WriteLine($"  IP Address: {networkConfig.IPAddress}");
                Console.WriteLine($"  Subnet Mask: {networkConfig.SubnetMask}");
                Console.WriteLine($"  Gateway: {networkConfig.Gateway}");
                Console.WriteLine($"  DNS Server: {networkConfig.DNSServer}");
                Console.WriteLine();

                // Get the active Wi-Fi adapter
                var adapterName = networkService.GetActiveWifiAdapter();
                if (string.IsNullOrEmpty(adapterName))
                {
                    throw new InvalidOperationException("No active Wi-Fi adapter found.");
                }

                Console.WriteLine($"Using adapter: {adapterName}");
                Console.WriteLine();

                // Set static IP
                var success = networkService.SetStaticIP(adapterName, networkConfig);
                if (success)
                {
                    Console.WriteLine("Static IP configuration applied successfully!");
                }
                else
                {
                    throw new InvalidOperationException("Failed to apply static IP configuration.");
                }
            }
            else
            {
                // Apply DHCP configuration
                Console.WriteLine($"Network '{currentSSID}' not found in configuration.");
                Console.WriteLine("Applying DHCP (dynamic IP)...");
                Console.WriteLine();

                // Get the active Wi-Fi adapter
                var adapterName = networkService.GetActiveWifiAdapter();
                if (string.IsNullOrEmpty(adapterName))
                {
                    throw new InvalidOperationException("No active Wi-Fi adapter found.");
                }

                Console.WriteLine($"Using adapter: {adapterName}");
                Console.WriteLine();

                // Set DHCP
                var success = networkService.SetDHCP(adapterName);
                if (success)
                {
                    Console.WriteLine("DHCP configuration applied successfully!");
                }
                else
                {
                    throw new InvalidOperationException("Failed to apply DHCP configuration.");
                }
            }
        }
        catch (PlatformNotSupportedException ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Supported platforms: Windows, Linux");
            Environment.Exit(1);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine();

            // Provide platform-specific guidance
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("Please ensure:");
                Console.WriteLine("  - You are connected to a Wi-Fi network");
                Console.WriteLine("  - You are running this application as Administrator");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("Please ensure:");
                Console.WriteLine("  - You are connected to a Wi-Fi network");
                Console.WriteLine("  - You are running this application with sudo (root privileges)");
                Console.WriteLine();
                Console.WriteLine("Required tools (install if missing):");
                Console.WriteLine("  Ubuntu/Debian: sudo apt-get install iproute2 wireless-tools network-manager");
                Console.WriteLine("  Fedora/RHEL: sudo dnf install iproute wireless-tools NetworkManager");
                Console.WriteLine("  Arch Linux: sudo pacman -S iproute2 wireless_tools networkmanager");
            }

            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: An unexpected error occurred: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Stack trace:");
            Console.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
