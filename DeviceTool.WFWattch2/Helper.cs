namespace DeviceTool.WFWattch2;

internal static class Helper
{
    public static IPAddress ResolveHost(string host)
    {
        try
        {
            return IPAddress.Parse(host);
        }
        catch (FormatException)
        {
            return Dns.GetHostEntry(host).AddressList[0];
        }
    }
}
