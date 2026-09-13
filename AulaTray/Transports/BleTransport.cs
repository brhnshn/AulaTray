using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace AulaTray.Transports;

public class BleTransport : IKeyboardTransport
{
    public ConnectionMode Mode => ConnectionMode.Bluetooth;

    public bool TryDetect(string[] hidPaths, out TransportDeviceInfo? info)
    {
        // Bluetooth cihazları BTHLE üzerinden SetupAPI ile aranır
        info = null;
        return false;
    }

    public KeyboardStatus? QueryStatus(TransportDeviceInfo info, BatteryFilter filter)
    {
        var btStatus = QueryBluetoothStatus(filter.LastKnownBattery);
        if (btStatus.isConnected)
        {
            int filteredBattery = filter.Filter(btStatus.battery, PowerState.Discharging);
            return new KeyboardStatus(
                filteredBattery,
                PowerState.Discharging,
                ConnectionState.Connected,
                ConnectionMode.Bluetooth,
                btStatus.modelName
            );
        }

        return null;
    }

    public (bool isConnected, int battery, string modelName) QueryBluetoothStatus(int fallbackBattery)
    {
        try
        {
            var task = Task.Run(() => QueryBluetoothStatusAsync(fallbackBattery));
            if (task.Wait(300))
            {
                return task.Result;
            }
        }
        catch { }

        return (false, 0, ModelRegistry.DefaultModelName);
    }

    private static async Task<(bool isConnected, int battery, string modelName)> QueryBluetoothStatusAsync(int fallbackBattery)
    {
        IntPtr devInfo = HidNativeMethods.SetupDiGetClassDevs(IntPtr.Zero, "BTHLE", IntPtr.Zero, 0x02 | 0x04);
        if (devInfo == IntPtr.Zero || devInfo == new IntPtr(-1))
            return (false, 0, ModelRegistry.DefaultModelName);

        try
        {
            SP_DEVINFO_DATA devData = new SP_DEVINFO_DATA();
            devData.cbSize = Marshal.SizeOf<SP_DEVINFO_DATA>();

            uint index = 0;
            while (HidNativeMethods.SetupDiEnumDeviceInfo(devInfo, index++, ref devData))
            {
                DEVPROPKEY pkey = HidNativeMethods.PKEY_NAME;
                byte[] buf = new byte[512];
                if (HidNativeMethods.SetupDiGetDevicePropertyW(devInfo, ref devData, ref pkey, out _, buf, (uint)buf.Length, out _, 0))
                {
                    string name = Encoding.Unicode.GetString(buf).TrimEnd('\0');
                    bool isAulaOrKnown = name.Contains("AULA", StringComparison.OrdinalIgnoreCase) ||
                        ModelRegistry.GetAllModels().Any(m => m.BtKeywords.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase)));

                    if (isAulaOrKnown)
                    {
                        string matchedModel = ModelRegistry.MatchBluetoothDevice(name);
                        StringBuilder idBuilder = new StringBuilder(256);
                        HidNativeMethods.SetupDiGetDeviceInstanceIdW(devInfo, ref devData, idBuilder, idBuilder.Capacity, out _);
                        string instanceId = idBuilder.ToString();

                        int devIdx = instanceId.IndexOf("DEV_", StringComparison.OrdinalIgnoreCase);
                        if (devIdx >= 0 && instanceId.Length >= devIdx + 16)
                        {
                            string hexAddr = instanceId.Substring(devIdx + 4, 12);
                            if (ulong.TryParse(hexAddr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong addr))
                            {
                                var ble = await BluetoothLEDevice.FromBluetoothAddressAsync(addr);
                                if (ble != null && ble.ConnectionStatus == BluetoothConnectionStatus.Connected)
                                {
                                    int bat = await ReadGattBatteryAsync(ble);
                                    if (bat <= 0)
                                    {
                                        DEVPROPKEY pkeyBat = HidNativeMethods.PKEY_Device_BatteryLevel;
                                        byte[] batBuf = new byte[16];
                                        if (HidNativeMethods.SetupDiGetDevicePropertyW(devInfo, ref devData, ref pkeyBat, out _, batBuf, (uint)batBuf.Length, out _, 0))
                                        {
                                            if (batBuf.Length > 0 && batBuf[0] > 0 && batBuf[0] <= 100)
                                            {
                                                bat = batBuf[0];
                                            }
                                        }
                                    }

                                    return (true, bat > 0 ? bat : fallbackBattery, matchedModel);
                                }
                            }
                        }
                    }
                }
            }
        }
        catch { }
        finally
        {
            HidNativeMethods.SetupDiDestroyDeviceInfoList(devInfo);
        }

        return (false, 0, ModelRegistry.DefaultModelName);
    }

    private static async Task<int> ReadGattBatteryAsync(BluetoothLEDevice ble)
    {
        try
        {
            var gatt = await ble.GetGattServicesForUuidAsync(GattServiceUuids.Battery);
            if (gatt.Status == GattCommunicationStatus.Success)
            {
                foreach (var s in gatt.Services)
                {
                    var chars = await s.GetCharacteristicsForUuidAsync(GattCharacteristicUuids.BatteryLevel);
                    if (chars.Status == GattCommunicationStatus.Success)
                    {
                        foreach (var c in chars.Characteristics)
                        {
                            var val = await c.ReadValueAsync();
                            if (val.Status == GattCommunicationStatus.Success)
                            {
                                var reader = DataReader.FromBuffer(val.Value);
                                return reader.ReadByte();
                            }
                        }
                    }
                }
            }
        }
        catch { }
        return 0;
    }
}
