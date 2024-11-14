# RS-WFWATTCH2 library

| Package | Info |
|:-|:-|
| DeviceLib.WFWattch2 | [![NuGet](https://img.shields.io/nuget/v/DeviceLib.WFWattch2.svg)](https://www.nuget.org/packages/DeviceLib.WFWattch2) |

## Usage

```csharp
using DeviceLib.WFWattch2;

using var client = new WattchClient(IPAddress.Parse("192.168.1.101"));
await client.ConnectAsync(cts.Token).ConfigureAwait(false);

using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
while (await timer.WaitForNextTickAsync().ConfigureAwait(false))
{
    if (await client.UpdateAsync(cts.Token).ConfigureAwait(false))
    {
        Console.WriteLine(
            $"{client.DateTime:yyyy/MM/dd HH:mm:ss}: " +
            $"Power={client.Power:F2}W, " +
            $"Voltage={client.Voltage:F2}V, " +
            $"Current={client.Current * 1000.0:F0}mA");
    }
}
```

## Globalt tool

### Install

```
> dotnet tool install -g DeviceTool.WFWattch2
```

### Usage

```
wfwatch2 measure -h 192.168.1.101
```
