using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace AulaTray;

public class HidService
{
    private const ushort DongleVendorId = 0x3554;
    private const ushort DongleProductId = 0xFA09;

    private const ushort WiredVendorId = 0x258A;
    private const ushort WiredProductId = 0x010C;

    [StructLayout(LayoutKind.Sequential)]
    private struct DEVPROPKEY
    {
        public Guid fmtid;
        public uint pid;
    }

    private static readonly DEVPROPKEY PKEY_Device_BatteryLevel = new DEVPROPKEY
    {
        fmtid = new Guid("104EA319-6EE2-4701-BD47-8DDBF425BBE5"),
        pid = 2
    };

    private static readonly DEVPROPKEY PKEY_NAME = new DEVPROPKEY
    {
        fmtid = new Guid("b725f130-47ef-101a-a5f1-02608c9eebac"),
        pid = 10
    };

    [StructLayout(LayoutKind.Sequential)]
    private struct SP_DEVINFO_DATA
    {
        public int cbSize;
        public Guid ClassGuid;
        public int DevInst;
        public IntPtr Reserved;
    }

    [DllImport("hid.dll")]
    private static extern void HidD_GetHidGuid(out Guid hidGuid);

    [DllImport("cfgmgr32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int CM_Get_Device_Interface_List_Size(out uint pulLen, ref Guid pInterfaceClassGuid, string? pDeviceID, uint ulFlags);

    [DllImport("cfgmgr32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int CM_Get_Device_Interface_List(ref Guid pInterfaceClassGuid, string? pDeviceID, char[] Buffer, uint BufferLen, uint ulFlags);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern IntPtr SetupDiGetClassDevs(IntPtr ClassGuid, string? Enumerator, IntPtr hwndParent, uint Flags);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, uint MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);

    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool SetupDiGetDevicePropertyW(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, ref DEVPROPKEY propertyKey, out uint propertyType, byte[]? propertyBuffer, uint propertyBufferSize, out uint requiredSize, uint flags);

    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool SetupDiGetDeviceInstanceIdW(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, StringBuilder? DeviceInstanceId, int DeviceInstanceIdSize, out int RequiredSize);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteFile(SafeFileHandle hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadFile(SafeFileHandle hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

    [DllImport("hid.dll", SetLastError = true)]
    private static extern bool HidD_GetFeature(SafeFileHandle hidDeviceObject, byte[] lpReportBuffer, int reportBufferLength);

    [DllImport("hid.dll", SetLastError = true)]
    private static extern bool HidD_SetFeature(SafeFileHandle hidDeviceObject, byte[] lpReportBuffer, int reportBufferLength);

    private int _lastKnownBattery = 90;
    private int _consecutiveDongleFailures = 0;

    public KeyboardStatus QueryStatus()
    {
        string[] allPaths = GetAllHidPaths();

        // 1. Kablolu (USB) Mod Kontrolü
        string? wiredPath = FindWiredPath(allPaths);
        if (!string.IsNullOrEmpty(wiredPath))
        {
            _consecutiveDongleFailures = 0;
            int wiredBattery = QueryWiredBattery(wiredPath);
            if (wiredBattery > 0) _lastKnownBattery = wiredBattery;

            return new KeyboardStatus(_lastKnownBattery, PowerState.Charging, ConnectionState.Connected, ConnectionMode.Wired);
        }

        // 2. Bluetooth Mod Kontrolü (AULA Bluetooth cihazı aktif bağlı mı?)
        var btStatus = QueryBluetoothStatus();
        if (btStatus.isConnected)
        {
            _consecutiveDongleFailures = 0;
            if (btStatus.battery > 0) _lastKnownBattery = btStatus.battery;
            return new KeyboardStatus(_lastKnownBattery, PowerState.Discharging, ConnectionState.Connected, ConnectionMode.Bluetooth);
        }

        // 3. 2.4 GHz Dongle Mod Kontrolü
        string? donglePath = FindDonglePath(allPaths);
        if (!string.IsNullOrEmpty(donglePath))
        {
            KeyboardStatus? dongleStatus = QueryDongleStatusSafe(donglePath);
            if (dongleStatus != null)
            {
                _consecutiveDongleFailures = 0;
                if (dongleStatus.BatteryPercent > 0)
                {
                    _lastKnownBattery = dongleStatus.BatteryPercent;
                }
                return dongleStatus;
            }

            _consecutiveDongleFailures++;
            if (_consecutiveDongleFailures < 4)
            {
                // Tekil sorgu gecikmesinde hemen uykuda deme, bağlantıyı koru
                return new KeyboardStatus(_lastKnownBattery, PowerState.Discharging, ConnectionState.Connected, ConnectionMode.Wireless24G);
            }

            // Art arda 4 periyot (yaklaşık 1 dakika) RF yanıtı dönmezse uykuda göster
            return new KeyboardStatus(_lastKnownBattery, PowerState.Discharging, ConnectionState.Sleeping, ConnectionMode.Wireless24G);
        }

        // 4. Hiçbir Aygıt Takılı Değil
        return new KeyboardStatus(0, PowerState.Discharging, ConnectionState.Disconnected, ConnectionMode.Disconnected);
    }

    private (bool isConnected, int battery) QueryBluetoothStatus()
    {
        try
        {
            var task = Task.Run(QueryBluetoothStatusAsync);
            if (task.Wait(300))
            {
                return task.Result;
            }
        }
        catch { }

        return (false, 0);
    }

    private async Task<(bool isConnected, int battery)> QueryBluetoothStatusAsync()
    {
        IntPtr devInfo = SetupDiGetClassDevs(IntPtr.Zero, "BTHLE", IntPtr.Zero, 0x02 | 0x04);
        if (devInfo == IntPtr.Zero || devInfo == new IntPtr(-1))
            return (false, 0);

        try
        {
            SP_DEVINFO_DATA devData = new SP_DEVINFO_DATA();
            devData.cbSize = Marshal.SizeOf<SP_DEVINFO_DATA>();

            uint index = 0;
            while (SetupDiEnumDeviceInfo(devInfo, index++, ref devData))
            {
                DEVPROPKEY pkey = PKEY_NAME;
                byte[] buf = new byte[512];
                if (SetupDiGetDevicePropertyW(devInfo, ref devData, ref pkey, out _, buf, (uint)buf.Length, out _, 0))
                {
                    string name = Encoding.Unicode.GetString(buf).TrimEnd('\0');
                    if (name.Contains("AULA", StringComparison.OrdinalIgnoreCase) || name.Contains("F75", StringComparison.OrdinalIgnoreCase))
                    {
                        StringBuilder idBuilder = new StringBuilder(256);
                        SetupDiGetDeviceInstanceIdW(devInfo, ref devData, idBuilder, idBuilder.Capacity, out _);
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
                                    // Pil seviyesini oku
                                    int bat = await ReadGattBatteryAsync(ble);
                                    if (bat <= 0)
                                    {
                                        DEVPROPKEY pkeyBat = PKEY_Device_BatteryLevel;
                                        byte[] batBuf = new byte[16];
                                        if (SetupDiGetDevicePropertyW(devInfo, ref devData, ref pkeyBat, out _, batBuf, (uint)batBuf.Length, out _, 0))
                                        {
                                            if (batBuf.Length > 0 && batBuf[0] > 0 && batBuf[0] <= 100)
                                            {
                                                bat = batBuf[0];
                                            }
                                        }
                                    }

                                    return (true, bat > 0 ? bat : _lastKnownBattery);
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
            SetupDiDestroyDeviceInfoList(devInfo);
        }

        return (false, 0);
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

    private KeyboardStatus? QueryDongleStatusSafe(string devicePath)
    {
        try
        {
            var task = Task.Run(() => QueryDongleStatus(devicePath));
            if (task.Wait(800))
            {
                return task.Result;
            }
        }
        catch { }

        return null;
    }

    private KeyboardStatus? QueryDongleStatus(string devicePath)
    {
        try
        {
            using SafeFileHandle handle = CreateFile(
                devicePath,
                0xC0000000,
                0x00000001 | 0x00000002,
                IntPtr.Zero,
                3,
                0,
                IntPtr.Zero
            );

            if (handle.IsInvalid) return null;

            byte[] request = new byte[20];
            request[0] = 0x13;
            request[1] = 0x4A;
            byte sum = 0;
            for (int i = 0; i < 19; i++) sum = (byte)(sum + request[i]);
            request[19] = sum;

            if (!WriteFile(handle, request, (uint)request.Length, out _, IntPtr.Zero))
            {
                return null;
            }

            byte[] buffer = new byte[64];
            for (int attempt = 0; attempt < 3; attempt++)
            {
                if (ReadFile(handle, buffer, (uint)buffer.Length, out uint bytesRead, IntPtr.Zero) && bytesRead >= 7)
                {
                    if (buffer[0] == 0x13 && buffer[1] == 0x4A)
                    {
                        int battery = buffer[5];
                        PowerState power = (buffer[6] == 0x10) ? PowerState.Charging : PowerState.Discharging;
                        if (battery > 100) battery = 100;
                        if (battery <= 0) battery = _lastKnownBattery;

                        return new KeyboardStatus(battery, power, ConnectionState.Connected, ConnectionMode.Wireless24G);
                    }
                }
            }
        }
        catch
        {
        }

        return null;
    }

    private int QueryWiredBattery(string devicePath)
    {
        try
        {
            using SafeFileHandle handle = CreateFile(
                devicePath,
                0xC0000000,
                0x00000001 | 0x00000002,
                IntPtr.Zero,
                3,
                0,
                IntPtr.Zero
            );

            if (handle.IsInvalid) return 0;

            byte[] req = new byte[520];
            req[0] = 0x06;
            req[1] = 0x84;
            req[4] = 0x01;
            req[6] = 0x80;

            if (HidD_SetFeature(handle, req, req.Length))
            {
                byte[] resp = new byte[520];
                resp[0] = 0x06;
                if (HidD_GetFeature(handle, resp, resp.Length))
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

        return _lastKnownBattery;
    }

    private string[] GetAllHidPaths()
    {
        HidD_GetHidGuid(out Guid hidGuid);
        if (CM_Get_Device_Interface_List_Size(out uint len, ref hidGuid, null, 0) != 0 || len == 0)
            return Array.Empty<string>();

        char[] buffer = new char[len];
        if (CM_Get_Device_Interface_List(ref hidGuid, null, buffer, len, 0) != 0)
            return Array.Empty<string>();

        string allPaths = new string(buffer);
        return allPaths.Split('\0', StringSplitOptions.RemoveEmptyEntries);
    }

    private string? FindDonglePath(string[] paths)
    {
        foreach (string path in paths)
        {
            if (path.Contains("VID_3554", StringComparison.OrdinalIgnoreCase) &&
                path.Contains("PID_FA09", StringComparison.OrdinalIgnoreCase) &&
                path.Contains("Col01", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }
        }
        return null;
    }

    private string? FindWiredPath(string[] paths)
    {
        foreach (string path in paths)
        {
            if (path.Contains("VID_258A", StringComparison.OrdinalIgnoreCase) &&
                path.Contains("PID_010C", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }
        }
        return null;
    }
}
