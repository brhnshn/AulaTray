using System;
using AulaTray.Transports;

namespace AulaTray;

/// <summary>
/// Tüm donanım taşıyıcılarını (Kablolu, Dongle, Bluetooth) koordine eden
/// ve pil filtresini işleten ana HID servis cephesi (Facade).
/// </summary>
public class HidService
{
    private readonly BatteryFilter _filter;
    private readonly WiredTransport _wiredTransport;
    private readonly DongleTransport _dongleTransport;
    private readonly BleTransport _bleTransport;
    private string _lastModelName = ModelRegistry.DefaultModelName;

    public BatteryFilter Filter => _filter;

    public HidService(BatteryFilter? filter = null)
    {
        _filter = filter ?? new BatteryFilter();
        _wiredTransport = new WiredTransport();
        _dongleTransport = new DongleTransport();
        _bleTransport = new BleTransport();
    }

    public KeyboardStatus QueryStatus()
    {
        string[] allPaths = HidNativeMethods.GetAllHidPaths();

        // 1. Kablolu (USB) Mod Kontrolü
        if (_wiredTransport.TryDetect(allPaths, out var wiredInfo) && wiredInfo != null)
        {
            _dongleTransport.ResetFailures();
            _lastModelName = wiredInfo.ModelName;
            var status = _wiredTransport.QueryStatus(wiredInfo, _filter);
            if (status != null) return status;
        }

        // 2. 2.4 GHz Dongle Mod Kontrolü (Hızlı ve en yaygın mod)
        if (_dongleTransport.TryDetect(allPaths, out var dongleInfo) && dongleInfo != null)
        {
            _lastModelName = dongleInfo.ModelName;
            var status = _dongleTransport.QueryStatus(dongleInfo, _filter);
            if (status != null) return status;
        }

        // 3. Bluetooth Mod Kontrolü (Yalnızca kablo ve dongle yokken taranır)
        var btStatus = _bleTransport.QueryBluetoothStatus(_filter.LastKnownBattery);
        if (btStatus.isConnected)
        {
            _dongleTransport.ResetFailures();
            _lastModelName = btStatus.modelName;
            int filtered = _filter.Filter(btStatus.battery, PowerState.Discharging);
            return new KeyboardStatus(
                filtered,
                PowerState.Discharging,
                ConnectionState.Connected,
                ConnectionMode.Bluetooth,
                _lastModelName
            );
        }

        // 4. Bağlantı Yok
        return new KeyboardStatus(0, PowerState.Discharging, ConnectionState.Disconnected, ConnectionMode.Disconnected, _lastModelName);
    }
}
