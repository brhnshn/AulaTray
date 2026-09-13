namespace AulaTray.Transports;

public record TransportDeviceInfo(
    string DevicePath,
    string ModelName,
    ConnectionMode Mode
);

/// <summary>
/// Klavye donanım haberleşme taşıyıcı arayüzü.
/// Her bağlantı türü (Kablolu, 2.4G Dongle, BLE) bu sözleşmeyi uygular.
/// </summary>
public interface IKeyboardTransport
{
    ConnectionMode Mode { get; }

    /// <summary>
    /// Verilen HID yolları veya sistem aygıtları arasında bu taşıyıcıya uygun cihaz var mı?
    /// </summary>
    bool TryDetect(string[] hidPaths, out TransportDeviceInfo? info);

    /// <summary>
    /// Algılanan cihazdan pil ve durum verilerini okur.
    /// </summary>
    KeyboardStatus? QueryStatus(TransportDeviceInfo info, BatteryFilter filter);
}
