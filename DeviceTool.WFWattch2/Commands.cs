// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable MemberCanBePrivate.Global
namespace DeviceTool.WFWattch2;

using DeviceLib.WFWattch2;

using Smart.CommandLine.Hosting;

public static class CommandBuilderExtensions
{
    public static void AddCommands(this ICommandBuilder commands)
    {
        commands.AddCommand<MeasureCommand>();
    }
}

// Measure
[Command("measure", Description = "Measure")]
public sealed class MeasureCommand : ICommandHandler
{
    [Option<string>("--host", "-h", Description = "Host", IsRequired = true)]
    public string Host { get; set; } = default!;

    public async ValueTask ExecuteAsync(CommandContext context)
    {
        using var client = new WattchClient(Helper.ResolveHost(Host));

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(5000);

        await client.ConnectAsync(cts.Token).ConfigureAwait(false);

        if (await client.UpdateAsync(cts.Token).ConfigureAwait(false))
        {
            Console.WriteLine($"DateTime : {client.LastUpdate:yyyy/MM/dd HH:mm:ss}");
            Console.WriteLine($"Power    : {client.Power:F2}");
            Console.WriteLine($"Voltage  : {client.Voltage:F2}");
            Console.WriteLine($"Current  : {client.Current * 1000.0:F0}");
        }
    }
}
