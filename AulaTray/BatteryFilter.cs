namespace AulaTray;

/// <summary>
/// Lityum pillerde yük kalktığında oluşan voltaj toparlanması (sag rebound) sıçramalarını
/// filtreleyen ve gerçek şarj artışlarını onaylayan saf durum makinesi (Pure State Machine).
/// Hiçbir işletim sistemi veya donanım bağımlılığı yoktur, %100 birim test edilebilir.
/// </summary>
public class BatteryFilter
{
    public const int DefaultInitialBattery = 90;
    public const int ReboundJumpThreshold = 8;
    public const int SustainedHighReadingsRequired = 5;

    public int LastKnownBattery { get; private set; } = DefaultInitialBattery;
    public int ConsecutiveHighReadings { get; private set; } = 0;
    public int PendingHighBattery { get; private set; } = 0;

    public BatteryFilter(int initialBattery = DefaultInitialBattery)
    {
        LastKnownBattery = (initialBattery > 0 && initialBattery <= 100) ? initialBattery : DefaultInitialBattery;
    }

    public void Reset(int battery = DefaultInitialBattery)
    {
        LastKnownBattery = (battery > 0 && battery <= 100) ? battery : DefaultInitialBattery;
        ConsecutiveHighReadings = 0;
        PendingHighBattery = 0;
    }

    /// <summary>
    /// Ham donanım pil seviyesini mevcut güç durumuna göre filtreler.
    /// </summary>
    public int Filter(int rawBattery, PowerState powerState)
    {
        if (rawBattery <= 0 || rawBattery > 100)
            return LastKnownBattery;

        // 1. Şarj Oluyor Modu: Pilin yükselmesi beklenen ve doğal durumdur.
        if (powerState == PowerState.Charging)
        {
            ConsecutiveHighReadings = 0;
            PendingHighBattery = 0;
            LastKnownBattery = rawBattery;
            return LastKnownBattery;
        }

        // 2. Pilde Çalışıyor Modu (Discharging):
        // Eğer okunan değer mevcut değerden küçük veya eşitse doğal deşarj oluyordur.
        if (rawBattery <= LastKnownBattery)
        {
            ConsecutiveHighReadings = 0;
            PendingHighBattery = 0;
            LastKnownBattery = rawBattery;
            return LastKnownBattery;
        }

        // 3. Pildeyken değerin YÜKSELMESİ durumu (Örn: 81'den 84'e çıkması):
        // Bariz büyük bir artış varsa (>= 8%) kullanıcı klavyeyi kapalıyken şarja takıp doldurmuş olabilir.
        int diff = rawBattery - LastKnownBattery;
        if (diff >= ReboundJumpThreshold)
        {
            ConsecutiveHighReadings = 0;
            PendingHighBattery = 0;
            LastKnownBattery = rawBattery;
            return LastKnownBattery;
        }

        // Küçük sıçramalar (1-7%): Lityum pilin toparlanması (rebound).
        // 5 periyot boyunca stabil kalıp kalmadığını kontrol ediyoruz.
        if (PendingHighBattery == rawBattery)
        {
            ConsecutiveHighReadings++;
        }
        else
        {
            PendingHighBattery = rawBattery;
            ConsecutiveHighReadings = 1;
        }

        // Eğer arka arkaya 5 periyot boyunca yüksek gelmeye devam ederse gerçek şarj kabul edilir:
        if (ConsecutiveHighReadings >= SustainedHighReadingsRequired)
        {
            LastKnownBattery = rawBattery;
            ConsecutiveHighReadings = 0;
            PendingHighBattery = 0;
            return LastKnownBattery;
        }

        // Aksi halde sahte sıçramayı filtrele, kararlı değeri koru:
        return LastKnownBattery;
    }
}
