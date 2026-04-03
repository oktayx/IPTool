using System.Net.NetworkInformation;
using IPTool.Models;

namespace IPTool.Services;

public class NetworkVerificationService
{
    public class VerificationResult
    {
        public bool Success { get; set; }
        public List<string> Warnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    public async Task<VerificationResult> VerifyNetworkConfigAsync(NetworkConfig config)
    {
        var result = new VerificationResult { Success = true };

        await CheckIPConflictAsync(config.IPAddress, result);
        await CheckGatewayReachabilityAsync(config.Gateway, result);
        await CheckDNSReachabilityAsync(config.DNSServer, result);

        if (result.Errors.Count > 0)
        {
            result.Success = false;
        }

        return result;
    }

    private async Task CheckIPConflictAsync(string ipAddress, VerificationResult result)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(ipAddress, 2000);

            if (reply.Status == IPStatus.Success)
            {
                result.Errors.Add($"IP address {ipAddress} is already in use by another device on the network.");
            }
        }
        catch (PingException ex)
        {
            result.Warnings.Add($"Could not verify IP conflict for {ipAddress}: {ex.Message}");
        }
        catch (Exception ex)
        {
            result.Warnings.Add($"IP conflict check failed for {ipAddress}: {ex.Message}");
        }
    }

    private async Task CheckGatewayReachabilityAsync(string gateway, VerificationResult result)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(gateway, 2000);

            if (reply.Status != IPStatus.Success)
            {
                result.Warnings.Add($"Gateway {gateway} is not reachable (Status: {reply.Status}). Network may not work properly.");
            }
        }
        catch (PingException ex)
        {
            result.Warnings.Add($"Could not verify gateway reachability for {gateway}: {ex.Message}");
        }
        catch (Exception ex)
        {
            result.Warnings.Add($"Gateway reachability check failed for {gateway}: {ex.Message}");
        }
    }

    private async Task CheckDNSReachabilityAsync(string dnsServer, VerificationResult result)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(dnsServer, 2000);

            if (reply.Status != IPStatus.Success)
            {
                result.Warnings.Add($"DNS server {dnsServer} is not reachable (Status: {reply.Status}). DNS resolution may not work.");
            }
        }
        catch (PingException ex)
        {
            result.Warnings.Add($"Could not verify DNS server reachability for {dnsServer}: {ex.Message}");
        }
        catch (Exception ex)
        {
            result.Warnings.Add($"DNS server reachability check failed for {dnsServer}: {ex.Message}");
        }
    }
}
