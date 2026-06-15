namespace DeviceTool.WFWattch2;

internal static class Helper
{
    public static IPAddress ResolveHost(string host)
    {
        if (IPAddress.TryParse(host, out var address))
        {
            return address;
        }

        var addresses = Dns.GetHostEntry(host).AddressList;
        var ipv4 = Array.Find(addresses, static a => a.AddressFamily == AddressFamily.InterNetwork);
        return ipv4 ?? throw new InvalidOperationException($"No IPv4 address found. host=[{host}]");
    }
}
