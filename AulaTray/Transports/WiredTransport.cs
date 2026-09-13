using System;
using Microsoft.Win32.SafeHandles;

namespace AulaTray.Transports;

public class WiredTransport : IKeyboardTransport
{
    public ConnectionMode Mode => ConnectionMode.Wired;

    public bool TryDetect(string[] hidPaths, out TransportDeviceInfo? info)
    {
        foreach (string path in hidPaths)
        {
            if (ModelRegistry.IsAnyKnownWired(path))
            {
                string modelName = ModelRegistry.MatchWiredPath(path);
                info = new TransportDeviceInfo(path, modelName, ConnectionMode.Wired);
                return true;
            }
        }

        info = null;
        return false;
    }

    public KeyboardStatus? QueryStatus(TransportDeviceInfo info, BatteryFilter filter)
    {
        int rawBattery = QueryWiredBattery(info.DevicePath, filter.LastKnownBattery);
        int filteredBattery = filter.Filter(rawBattery, PowerState.Charging);

        return new KeyboardStatus(
            filteredBattery,
            PowerState.Charging,
            ConnectionState.Connected,
            ConnectionMode.Wired,
            info.ModelName
        );
    }

    private static int QueryWiredBattery(string devicePath, int fallbackBattery)
    {
        try
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

            if (handle.IsInvalid) return fallbackBattery;

            byte[] req = new byte[520];
            req[0] = 0x06;
            req[1] = 0x84;
            req[4] = 0x01;
            req[6] = 0x80;

            if (HidNativeMethods.HidD_SetFeature(handle, req, req.Length))
            {
                byte[] resp = new byte[520];
                resp[0] = 0x06;
                if (HidNativeMethods.HidD_GetFeature(handle, resp, resp.Length))
                {
                    if (resp.Length > 134)
                    {
                        int val = resp[134];
                        if (val >= 10 && val <= 100) return val;
                    }
                }
            }
        }
        catch
        {
        }

        return fallbackBattery;
    }
}
