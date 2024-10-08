namespace DeviceLib.WFWattch2;

using System.Net;
using System.Net.Sockets;

public sealed class WattchClient : IDisposable
{
    private const int Port = 60121;

    private static readonly byte[] MeasureCommand;

    private readonly IPEndPoint endPoint;

    // TODO keep connection

    public DateTime? DateTime { get; private set; }

    public double? Voltage { get; private set; }

    public double? Current { get; private set; }

    public double? Power { get; private set; }

#pragma warning disable CA1810
    static WattchClient()
    {
        MeasureCommand = MakeCommand([0x18, 0x00]);
    }
#pragma warning restore CA1810

    public WattchClient(IPAddress address)
    {
        endPoint = new IPEndPoint(address, Port);
    }

    public void Dispose()
    {
    }

    public async ValueTask UpdateAsync()
    {
        // TODO
        using var client = new TcpClient();
        await client.ConnectAsync(endPoint).ConfigureAwait(false);

        client.Client.Send(MeasureCommand);

        // TODO Fix
        var array = new byte[256];
        var read = client.Client.Receive(array);
        if (read < 0)
        {
            return;
        }

        if (read > 0)
        {
            var num47 = array[1] & 0xFF;
            if (num47 == 0)
            {
                Voltage = (double)(((long)array[10] << 40) + ((long)array[9] << 32) + ((long)array[8] << 24) + ((long)array[7] << 16) + ((long)array[6] << 8) + array[5]) / (1L << 24);
                Current = (double)(((long)array[16] << 40) + ((long)array[15] << 32) + ((long)array[14] << 24) + ((long)array[13] << 16) + ((long)array[12] << 8) + array[11]) / (1L << 30);
                Power = (double)(((long)array[22] << 40) + ((long)array[21] << 32) + ((long)array[20] << 24) + ((long)array[19] << 16) + ((long)array[18] << 8) + array[17]) / (1L << 24);
                DateTime = new DateTime(array[28] + 2000, array[27], array[26], array[25], array[24], array[23]);
            }
        }
    }

    public static byte[] MakeCommand(Span<byte> payload)
    {
        var array = new byte[payload.Length + 4];
        array[0] = 0xAA;
        array[1] = (byte)((uint)(payload.Length >> 8) & 0xFF);
        array[2] = (byte)((uint)payload.Length & 0xFF);
        payload.CopyTo(array.AsSpan(3));
        array[^1] = CalcCrc8(payload);
        return array;
    }

    private static byte CalcCrc8(Span<byte> payload)
    {
        var crc = 0;
        foreach (var b in payload)
        {
            crc = Table[(crc ^ b) & 0xFF];
        }
        return (byte)crc;
    }

    private static readonly byte[] Table =
    [
        0x00, 0x85, 0x8F, 0x0A, 0x9B, 0x1E, 0x14, 0x91, 0xB3, 0x36, 0x3C, 0xB9, 0x28, 0xAD, 0xA7, 0x22,
        0xE3, 0x66, 0x6C, 0xE9, 0x78, 0xFD, 0xF7, 0x72, 0x50, 0xD5, 0xDF, 0x5A, 0xCB, 0x4E, 0x44, 0xC1,
        0x43, 0xC6, 0xCC, 0x49, 0xD8, 0x5D, 0x57, 0xD2, 0xF0, 0x75, 0x7F, 0xFA, 0x6B, 0xEE, 0xE4, 0x61,
        0xA0, 0x25, 0x2F, 0xAA, 0x3B, 0xBE, 0xB4, 0x31, 0x13, 0x96, 0x9C, 0x19, 0x88, 0x0D, 0x07, 0x82,
        0x86, 0x03, 0x09, 0x8C, 0x1D, 0x98, 0x92, 0x17, 0x35, 0xB0, 0xBA, 0x3F, 0xAE, 0x2B, 0x21, 0xA4,
        0x65, 0xE0, 0xEA, 0x6F, 0xFE, 0x7B, 0x71, 0xF4, 0xD6, 0x53, 0x59, 0xDC, 0x4D, 0xC8, 0xC2, 0x47,
        0xC5, 0x40, 0x4A, 0xCF, 0x5E, 0xDB, 0xD1, 0x54, 0x76, 0xF3, 0xF9, 0x7C, 0xED, 0x68, 0x62, 0xE7,
        0x26, 0xA3, 0xA9, 0x2C, 0xBD, 0x38, 0x32, 0xB7, 0x95, 0x10, 0x1A, 0x9F, 0x0E, 0x8B, 0x81, 0x04,
        0x89, 0x0C, 0x06, 0x83, 0x12, 0x97, 0x9D, 0x18, 0x3A, 0xBF, 0xB5, 0x30, 0xA1, 0x24, 0x2E, 0xAB,
        0x6A, 0xEF, 0xE5, 0x60, 0xF1, 0x74, 0x7E, 0xFB, 0xD9, 0x5C, 0x56, 0xD3, 0x42, 0xC7, 0xCD, 0x48,
        0xCA, 0x4F, 0x45, 0xC0, 0x51, 0xD4, 0xDE, 0x5B, 0x79, 0xFC, 0xF6, 0x73, 0xE2, 0x67, 0x6D, 0xE8,
        0x29, 0xAC, 0xA6, 0x23, 0xB2, 0x37, 0x3D, 0xB8, 0x9A, 0x1F, 0x15, 0x90, 0x01, 0x84, 0x8E, 0x0B,
        0x0F, 0x8A, 0x80, 0x05, 0x94, 0x11, 0x1B, 0x9E, 0xBC, 0x39, 0x33, 0xB6, 0x27, 0xA2, 0xA8, 0x2D,
        0xEC, 0x69, 0x63, 0xE6, 0x77, 0xF2, 0xF8, 0x7D, 0x5F, 0xDA, 0xD0, 0x55, 0xC4, 0x41, 0x4B, 0xCE,
        0x4C, 0xC9, 0xC3, 0x46, 0xD7, 0x52, 0x58, 0xDD, 0xFF, 0x7A, 0x70, 0xF5, 0x64, 0xE1, 0xEB, 0x6E,
        0xAF, 0x2A, 0x20, 0xA5, 0x34, 0xB1, 0xBB, 0x3E, 0x1C, 0x99, 0x93, 0x16, 0x87, 0x02, 0x08, 0x8D
    ];
}
