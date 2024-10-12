using System.Net;

using DeviceLib.WFWattch2;

using var client = new WattchClient(IPAddress.Parse(args[0]));

using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
while (await timer.WaitForNextTickAsync().ConfigureAwait(false))
{
#pragma warning disable CA1031
    try
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(5000);

        if (!client.IsConnected())
        {
            await client.ConnectAsync(cts.Token).ConfigureAwait(false);
        }

        if (await client.UpdateAsync(cts.Token).ConfigureAwait(false))
        {
            Console.WriteLine($"{client.DateTime:yyyy/MM/dd HH:mm:ss}: Power={client.Power:F3}W, Voltage={client.Voltage:F3}V, Current={client.Current * 1000.0:F3}A");
        }
        else
        {
            client.Close();
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        client.Close();
    }
#pragma warning restore CA1031
}
