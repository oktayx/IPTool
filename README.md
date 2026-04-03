# IPTool - Wi-Fi IP Configuration Tool

A cross-platform command-line tool that automatically configures IP addresses based on the connected Wi-Fi network. It supports both Windows and Linux, allowing you to set static IP addresses for known networks and use DHCP for unknown networks.

## Features

- **Automatic Wi-Fi Detection**: Detects the currently connected Wi-Fi network
- **Static IP Configuration**: Sets static IP, subnet mask, gateway, and DNS for known networks
- **Dynamic IP (DHCP)**: Automatically uses DHCP for unknown networks
- **Cross-Platform Support**: Works on Windows and Linux
- **JSON Configuration**: Easy-to-edit configuration file for managing network settings
- **Platform-Specific Tools**: Uses native tools on each platform (netsh on Windows, nmcli/ip on Linux)

## Requirements

### Windows
- Windows 10 or later
- .NET 10.0 Runtime
- Administrator privileges

### Linux
- Any modern Linux distribution (Ubuntu, Debian, Fedora, Arch, etc.)
- .NET 10.0 Runtime
- Root privileges (sudo)

#### Required Linux Tools
The tool will try to use the following commands (install if missing):

```bash
# Ubuntu/Debian
sudo apt-get install iproute2 wireless-tools network-manager isc-dhcp-client

# Fedora/RHEL
sudo dnf install iproute wireless-tools NetworkManager dhcp-client

# Arch Linux
sudo pacman -S iproute2 wireless_tools networkmanager dhclient
```

## Installation

### Build from Source

1. Clone or download this repository
2. Build the project:
   ```bash
   dotnet build
   ```
3. The executable will be in `bin/Debug/net10.0/` (or `bin/Release/net10.0/` for release builds)

## Configuration

The tool uses a JSON configuration file (`wifi-config.json`) to define Wi-Fi networks and their IP settings. You must create this file in the application directory before running the tool.

### Configuration Format

```json
{
  "networks": {
    "MyWifi": {
      "ipAddress": "192.168.1.111",
      "subnetMask": "255.255.255.0",
      "gateway": "192.168.1.1",
      "dnsServer": "192.168.1.1"
    },
    "OfficeWiFi": {
      "ipAddress": "192.168.10.50",
      "subnetMask": "255.255.255.0",
      "gateway": "192.168.10.1",
      "dnsServer": "8.8.8.8"
    }
  }
}
```

### Adding a New Network

1. Open `wifi-config.json` in a text editor
2. Add a new entry under the `networks` object:
   ```json
   "YourWiFiName": {
     "ipAddress": "192.168.1.100",
     "subnetMask": "255.255.255.0",
     "gateway": "192.168.1.1",
     "dnsServer": "192.168.1.1"
   }
   ```
3. Save the file

## Usage

### Windows

1. Open Command Prompt or PowerShell as **Administrator**
2. Navigate to the application directory
3. Run the tool:
   ```bash
   IPTool.exe
   ```

### Linux

1. Open a terminal
2. Navigate to the application directory
3. Run the tool with sudo:
   ```bash
   sudo ./IPTool
   ```

### Example Output

#### Static IP Configuration (Known Network)

```
IPTool v1.0 - Wi-Fi IP Configuration Tool

Platform: Windows

Configuration loaded from: C:\Users\oktay\source\tools\IPTool\wifi-config.json

Detecting Wi-Fi network...
Connected to: Wifi47

Applying static IP configuration:
  IP Address: 192.168.1.111
  Subnet Mask: 255.255.255.0
  Gateway: 192.168.1.1
  DNS Server: 192.168.1.1

Using adapter: Wi-Fi

Static IP configuration applied successfully!
```

#### Dynamic IP Configuration (Unknown Network)

```
IPTool v1.0 - Wi-Fi IP Configuration Tool

Platform: Linux

Configuration loaded from: /home/user/IPTool/wifi-config.json

Detecting Wi-Fi network...
Connected to: CoffeeShopWiFi

Network 'CoffeeShopWiFi' not found in configuration.
Applying DHCP (dynamic IP)...

Using adapter: wlp3s0

DHCP configuration applied successfully!
```

## Troubleshooting

### Common Issues

#### "Configuration file not found: wifi-config.json"
The tool requires a configuration file to be present. Create a `wifi-config.json` file in the application directory with your network configurations. See the [Configuration](#configuration) section for the format.

### Windows

#### "No Wi-Fi adapter found or not connected to Wi-Fi"
- Ensure you are connected to a Wi-Fi network
- Make sure you're running the tool as Administrator

#### "Failed to apply static IP configuration"
- Verify the IP address, subnet mask, gateway, and DNS are correct
- Ensure the IP address is not already in use on the network
- Check that your Wi-Fi adapter is functioning properly

### Linux

#### "Command 'iwgetid' not found"
Install the required tools:
```bash
sudo apt-get install wireless-tools network-manager
```

#### "Failed to detect Wi-Fi SSID"
- Ensure you are connected to a Wi-Fi network
- Make sure you're running the tool with sudo
- Check that NetworkManager is running:
  ```bash
  sudo systemctl status NetworkManager
  ```

#### "Failed to apply static IP configuration"
- Verify the IP address, subnet mask, gateway, and DNS are correct
- Ensure the IP address is not already in use on the network
- Check that your Wi-Fi adapter is functioning properly
- Try using NetworkManager (nmcli) instead of the ip command

## Architecture

The tool uses a cross-platform architecture with the following components:

- **[`IPlatformNetworkService`](Services/IPlatformNetworkService.cs)**: Interface for platform-specific network operations
- **[`WindowsNetworkService`](Services/WindowsNetworkService.cs)**: Windows implementation using netsh
- **[`LinuxNetworkService`](Services/LinuxNetworkService.cs)**: Linux implementation using nmcli and ip commands
- **[`NetworkServiceFactory`](Services/NetworkServiceFactory.cs)**: Factory to create the appropriate service based on OS
- **[`ConfigManager`](Services/ConfigManager.cs)**: Manages loading and saving the JSON configuration
- **[`NetworkConfig`](Models/NetworkConfig.cs)**: Model for network configuration
- **[`WifiConfig`](Models/WifiConfig.cs)**: Model for Wi-Fi configuration containing multiple networks

For detailed architecture information, see [`plans/wifi-ip-tool-architecture.md`](plans/wifi-ip-tool-architecture.md).

## Project Structure

```
IPTool/
├── IPTool.csproj                      # Project file
├── Program.cs                         # Main application entry point
├── Models/
│   ├── NetworkConfig.cs               # Network configuration model
│   └── WifiConfig.cs                  # WiFi configuration model
├── Services/
│   ├── IPlatformNetworkService.cs     # Interface for platform-specific services
│   ├── NetworkServiceFactory.cs       # Factory to create platform-specific service
│   ├── WindowsNetworkService.cs       # Windows-specific network operations
│   ├── LinuxNetworkService.cs         # Linux-specific network operations
│   └── ConfigManager.cs               # Configuration management service
├── wifi-config.json                   # Configuration file (required, must be created)
├── README.md                          # This file

```

## Security Considerations

- The tool requires administrator/root privileges to change network settings
- Configuration file should be protected from unauthorized access
- Validate all IP addresses before applying them

## License

This project is provided as-is for personal use.

## Contributing

Feel free to submit issues, feature requests, or pull requests.


