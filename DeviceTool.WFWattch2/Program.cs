// ReSharper disable UseObjectOrCollectionInitializer
#pragma warning disable IDE0017
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

using DeviceLib.WFWattch2;

using DeviceTool.WFWattch2;

var rootCommand = new RootCommand("WFWATTCH2 tool");
rootCommand.AddGlobalOption(new Option<string>(["--host", "-h"], "Host") { IsRequired = true });

// Measure
var measureCommand = new Command("measure", "Measure");
measureCommand.Handler = CommandHandler.Create(async static (IConsole console, string host) =>
{
    using var client = new WattchClient(Helper.ResolveHost(host));

    using var cts = new CancellationTokenSource();
    cts.CancelAfter(5000);

    await client.ConnectAsync(cts.Token).ConfigureAwait(false);

    if (await client.UpdateAsync(cts.Token).ConfigureAwait(false))
    {
        console.WriteLine($"DateTime : {client.LastUpdate:yyyy/MM/dd HH:mm:ss}");
        console.WriteLine($"Power    : {client.Power:F2}");
        console.WriteLine($"Voltage  : {client.Voltage:F2}");
        console.WriteLine($"Current  : {client.Current * 1000.0:F0}");
    }
});
rootCommand.Add(measureCommand);

return await rootCommand.InvokeAsync(args).ConfigureAwait(false);
