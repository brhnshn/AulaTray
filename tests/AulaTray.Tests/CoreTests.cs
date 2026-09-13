using System;
using AulaTray;
using AulaTray.Transports;
using Xunit;

namespace AulaTray.Tests;

public class BatteryFilterTests
{
    [Fact]
    public void BatteryFilter_ShouldKeepInitialValue_WhenInitialized()
    {
        var filter = new BatteryFilter(initialBattery: 85);
        Assert.Equal(85, filter.LastKnownBattery);
    }

    [Fact]
    public void BatteryFilter_ShouldAcceptNormalDischarge_Immediately()
    {
        var filter = new BatteryFilter(initialBattery: 85);
        int result = filter.Filter(84, PowerState.Discharging);
        Assert.Equal(84, result);
        Assert.Equal(84, filter.LastKnownBattery);
    }

    [Fact]
    public void BatteryFilter_ShouldSuppressRebound_WhenJumpingSmallAmount()
    {
        // 81'den 84'e sıçrama (3% lityum toparlanması)
        var filter = new BatteryFilter(initialBattery: 81);

        int result1 = filter.Filter(84, PowerState.Discharging);
        Assert.Equal(81, result1);
        Assert.Equal(1, filter.ConsecutiveHighReadings);

        int result2 = filter.Filter(84, PowerState.Discharging);
        Assert.Equal(81, result2);
        Assert.Equal(2, filter.ConsecutiveHighReadings);
    }

    [Fact]
    public void BatteryFilter_ShouldAccept_After5ConsecutiveReadings()
    {
        var filter = new BatteryFilter(initialBattery: 81);

        // 1-4 periyot boyunca filtre eski değeri korur
        for (int i = 1; i <= 4; i++)
        {
            Assert.Equal(81, filter.Filter(84, PowerState.Discharging));
        }

        // 5. kararlı periyotta yeni değer kabul edilir
        int result5 = filter.Filter(84, PowerState.Discharging);
        Assert.Equal(84, result5);
        Assert.Equal(84, filter.LastKnownBattery);
    }

    [Fact]
    public void BatteryFilter_ShouldAcceptIncrease_ImmediatelyWhenCharging()
    {
        var filter = new BatteryFilter(initialBattery: 81);
        int result = filter.Filter(84, PowerState.Charging);
        Assert.Equal(84, result);
        Assert.Equal(84, filter.LastKnownBattery);
    }

    [Fact]
    public void BatteryFilter_ShouldAcceptLargeIncrease_Immediately()
    {
        // 81'den 90'a sıçrama (>= 8%)
        var filter = new BatteryFilter(initialBattery: 81);
        int result = filter.Filter(90, PowerState.Discharging);
        Assert.Equal(90, result);
        Assert.Equal(90, filter.LastKnownBattery);
    }
}

public class ModelRegistryTests
{
    [Fact]
    public void ModelRegistry_ShouldMatchDongleF75()
    {
        string path = @"\\?\hid#vid_3554&pid_fa09&mi_00&col01#7&308479e0&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}";
        string model = ModelRegistry.MatchDonglePath(path);
        Assert.Equal("Aula F75", model);
    }

    [Fact]
    public void ModelRegistry_ShouldMatchDongleF87()
    {
        string path = @"\\?\hid#vid_3554&pid_fa0a&mi_00&col01#7&12345678&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}";
        string model = ModelRegistry.MatchDonglePath(path);
        Assert.Equal("Aula F87", model);
    }

    [Fact]
    public void ModelRegistry_ShouldMatchDongleF99()
    {
        string path = @"\\?\hid#vid_3554&pid_fa11&mi_00&col01#7&87654321&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}";
        string model = ModelRegistry.MatchDonglePath(path);
        Assert.Equal("Aula F99", model);
    }

    [Fact]
    public void ModelRegistry_ShouldFallbackToGeneric_ForUnknownCompxDongle()
    {
        string path = @"\\?\hid#vid_3554&pid_9999&mi_00&col01#7&99999999&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}";
        string model = ModelRegistry.MatchDonglePath(path);
        Assert.Equal("Aula Wireless Keyboard", model);
    }

    [Fact]
    public void ModelRegistry_ShouldMatchBluetoothDevice_ByKeyword()
    {
        string model = ModelRegistry.MatchBluetoothDevice("AULA F75 BT");
        Assert.Equal("Aula F75", model);

        string model87 = ModelRegistry.MatchBluetoothDevice("Aula-F87-Keyboard");
        Assert.Equal("Aula F87", model87);
    }
}

public class DongleTransportTests
{
    [Fact]
    public void BuildRfQueryPacket_ShouldHaveValidChecksum()
    {
        byte[] packet = DongleTransport.BuildRfQueryPacket();
        Assert.Equal(20, packet.Length);
        Assert.Equal(0x13, packet[0]);
        Assert.Equal(0x4A, packet[1]);

        byte expectedSum = 0;
        for (int i = 0; i < 19; i++) expectedSum = (byte)(expectedSum + packet[i]);
        Assert.Equal(expectedSum, packet[19]);
    }
}
