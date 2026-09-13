using System;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace AulaTray.Transports;

public class DongleTransport : IKeyboardTransport
{
    public ConnectionMode Mode => ConnectionMode.Wireless24G;

    private int _consecutiveFailures = 0;

    public void ResetFailures() => _consecutiveFailures = 0;

    public bool TryDetect(string[] hidPaths, out TransportDeviceInfo? info)
    {
        foreach (string path in hidPaths)
        {
            if (ModelRegistry.IsAnyKnownDongle(path))
            {
                string modelName = ModelRegistry.MatchDonglePath(path);
                info = new TransportDeviceInfo(path, modelName, ConnectionMode.Wireless24G);
                return true;
            }
        }

        info = null;
        return false;
    }

    public KeyboardStatus? QueryStatus(TransportDeviceInfo info, BatteryFilter filter)
    {
        KeyboardStatus? status = QueryDongleStatusSafe(info.DevicePath, info.ModelName, filter.LastKnownBattery);
        if (status != null)
        {
            _consecutiveFailures = 0;
            int filteredBattery = filter.Filter(status.BatteryPercent, status.PowerState);
            return status with { BatteryPercent = filteredBattery };
        }

        _consecutiveFailures++;
        if (_consecutiveFailures < 2)
        {
            // Tekil gecikmede bağlantıyı koru
            return new KeyboardStatus(
                filter.LastKnownBattery,
                PowerState.Discharging,
                ConnectionState.Connected,
                ConnectionMode.Wireless24G,
                info.ModelName
            );
        }

        // 2 periyot yanıt gelmezse (~7s) klavye uykuda
        return new KeyboardStatus(
            filter.LastKnownBattery,
            PowerState.Discharging,
            ConnectionState.Sleeping,
            ConnectionMode.Wireless24G,
            info.ModelName
        );
    }

    public static byte[] BuildRfQueryPacket()
    {
        byte[] request = new byte[20];
        request[0] = 0x13;
        request[1] = 0x4A;
        byte sum = 0;
        for (int i = 0; i < 19; i++) sum = (byte)(sum + request[i]);
        request[19] = sum;
        return request;
    }

    private static KeyboardStatus? QueryDongleStatusSafe(string devicePath, string modelName, int lastKnownBattery)
    {
        SafeFileHandle? activeHandle = null;
        try
        {
            var task = Task.Run(() =>
            {
                using SafeFileHandle handle = HidNativeMethods.CreateFile(
                    devicePath,
                    0xC0000000,
                    0x00000001 | 0x00000002,
                    IntPtr.Zero,
                    3,
                    0,
                    IntPtr.Zero
                );
                activeHandle = handle;

                if (handle.IsInvalid) return null;

                byte[] request = BuildRfQueryPacket();
                if (!HidNativeMethods.WriteFile(handle, request, (uint)request.Length, out _, IntPtr.Zero))
                {
                    return null;
                }

                byte[] buffer = new byte[64];
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    if (HidNativeMethods.ReadFile(handle, buffer, (uint)buffer.Length, out uint bytesRead, IntPtr.Zero) && bytesRead >= 7)
                    {
                        if (buffer[0] == 0x13 && buffer[1] == 0x4A)
                        {
                            int battery = buffer[5];
                            PowerState power = (buffer[6] == 0x10) ? PowerState.Charging : PowerState.Discharging;
                            if (battery > 100) battery = 100;
                            if (battery <= 0) battery = lastKnownBattery;

                            return new KeyboardStatus(battery, power, ConnectionState.Connected, ConnectionMode.Wireless24G, modelName);
                        }
                    }
                }

                return null;
            });

            if (task.Wait(600))
            {
                return task.Result;
            }
            else
            {
                // Zaman aşımı: Askıda kalan ReadFile'ı hemen iptal ederek thread havuzunu serbest bırak
                if (activeHandle != null && !activeHandle.IsClosed && !activeHandle.IsInvalid)
                {
                    HidNativeMethods.CancelIoEx(activeHandle, IntPtr.Zero);
                }
            }
        }
        catch { }

        return null;
    }
}
