using System.Runtime.InteropServices;

namespace IPTool.Services;

/// <summary>
/// Factory for creating platform-specific network services.
/// </summary>
public static class NetworkServiceFactory
{
    /// <summary>
    /// Creates the appropriate network service for the current platform.
    /// </summary>
    /// <returns>An instance of IPlatformNetworkService.</returns>
    /// <exception cref="PlatformNotSupportedException">Thrown when the platform is not supported.</exception>
    public static IPlatformNetworkService CreateService()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsNetworkService();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new LinuxNetworkService();
        }
        else
        {
            throw new PlatformNotSupportedException(
                $"Unsupported operating system: {RuntimeInformation.OSDescription}. " +
                "This tool supports Windows and Linux only.");
        }
    }
}
