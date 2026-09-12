namespace AulaTray;

public enum PowerState
{
    Discharging,
    Charging
}

public enum ConnectionState
{
    Connected,
    Sleeping,
    Disconnected
}

public enum ConnectionMode
{
    Wired,
    Wireless24G,
    Bluetooth,
    Disconnected
}

public record KeyboardStatus(
    int BatteryPercent,
    PowerState PowerState,
    ConnectionState ConnectionState,
    ConnectionMode Mode = ConnectionMode.Disconnected
);
