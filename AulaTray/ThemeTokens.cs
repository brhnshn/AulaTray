using System.Drawing;

namespace AulaTray;

/// <summary>
/// 3 Katmanlı Tasarım Sistemi (Design Tokens)
/// 1. Katman: Primitives (Ham değerler, hex renkler, sabit ölçüler)
/// 2. Katman: Semantics (Anlamsal ve amaca yönelik takma adlar)
/// 3. Katman: Components (Bileşene özel durumlar ve fırçalar/kalemler)
/// </summary>
public static class ThemeTokens
{
    #region 1. İlkel Katman (Primitives)
    public static class Primitives
    {
        // Renk Paleti (Dark Slate & Emerald)
        public static readonly Color ColorGray950 = Color.FromArgb(21, 24, 27);   // #15181B (Ana arka plan)
        public static readonly Color ColorGray900 = Color.FromArgb(26, 30, 34);   // #1A1E22 (Gövde kartı)
        public static readonly Color ColorGray850 = Color.FromArgb(29, 34, 38);   // #1D2226 (Kapsül ve elemanlar)
        public static readonly Color ColorGray800 = Color.FromArgb(42, 47, 52);   // #2A2F34 (Çerçeveler ve yay izi)
        public static readonly Color ColorGray700 = Color.FromArgb(70, 75, 82);   // #464B52 (Devre dışı elemanlar)
        public static readonly Color ColorGray500 = Color.FromArgb(122, 130, 138); // #7A828A (Soluk metinler / uyku)
        public static readonly Color ColorGray400 = Color.FromArgb(150, 155, 165); // #969BA5 (Açık soluk metin)
        public static readonly Color ColorGray300 = Color.FromArgb(183, 190, 196); // #B7BEC4 (Kapsül metni)
        public static readonly Color ColorGray100 = Color.FromArgb(232, 235, 237); // #E8EBED (Ana beyazımsı metin)

        // Aksan Renkler
        public static readonly Color ColorGreenEmerald = Color.FromArgb(62, 190, 142); // #3EBE8E (Canlı yeşil)
        public static readonly Color ColorGreenMint    = Color.FromArgb(93, 202, 165); // #5DCAA5 (Yumuşak yeşil / marka)
        public static readonly Color ColorGreenBoxBg   = Color.FromArgb(31, 92, 74);   // #1F5C4A (Logo kutu arka planı)
        public static readonly Color ColorCyanCharging = Color.FromArgb(0, 210, 255);  // #00D2FF (Şarj olma mavisi)
        public static readonly Color ColorRedWarning   = Color.FromArgb(235, 85, 85);  // #EB5555 (Kritik pil / hata)

        // Yazı Tipi İsimleri
        public const string FontFamilySans = "Segoe UI";
        public const string FontFamilyMono = "Consolas";
    }
    #endregion

    #region 2. Anlamsal Katman (Semantics)
    public static class Semantics
    {
        // Yüzeyler (Surfaces)
        public static Color SurfaceBase       => Primitives.ColorGray950;
        public static Color SurfaceCard       => Primitives.ColorGray900;
        public static Color SurfacePill       => Primitives.ColorGray850;
        public static Color SurfaceBrandBox   => Primitives.ColorGreenBoxBg;

        // Çerçeveler (Borders)
        public static Color BorderSubtle      => Primitives.ColorGray800;
        public static Color BorderTrack       => Primitives.ColorGray800;

        // Tipografi (Text)
        public static Color TextPrimary       => Primitives.ColorGray100;
        public static Color TextSecondary     => Primitives.ColorGray300;
        public static Color TextMuted         => Primitives.ColorGray500;
        public static Color TextDisabled      => Primitives.ColorGray400;

        // Durum Göstergeleri (State Colors)
        public static Color StateActive       => Primitives.ColorGreenEmerald;
        public static Color StateBrand        => Primitives.ColorGreenMint;
        public static Color StateCharging     => Primitives.ColorCyanCharging;
        public static Color StateWarning      => Primitives.ColorRedWarning;
        public static Color StateSleeping     => Primitives.ColorGray500;
        public static Color StateDisconnected => Primitives.ColorGray700;
    }
    #endregion

    #region 3. Bileşen Katmanı (Components)
    public static class Components
    {
        // Form & Pencere
        public static readonly Size WindowSize = new Size(300, 190);
        public static Color WindowBackground => Semantics.SurfaceBase;

        // Logo / Marka Rozeti
        public static Color BrandBoxBg => Semantics.SurfaceBrandBox;
        public const float BrandBoxRadius = 7f;

        // Kapsül (Mode Pill)
        public static Color PillBackground   => Semantics.SurfacePill;
        public static Color PillBorderColor  => Semantics.BorderSubtle;
        public static Color PillTextColor    => Semantics.TextSecondary;
        public const float PillRadius = 6f;

        // Batarya Halkası (Ring Indicator)
        public static Color RingTrackColor => Semantics.BorderTrack;
        public const float RingStrokeWidth = 7f;

        public static Color GetRingProgressColor(KeyboardStatus status)
        {
            return status.ConnectionState switch
            {
                ConnectionState.Sleeping => Semantics.StateSleeping,
                ConnectionState.Disconnected => Semantics.StateDisconnected,
                _ => status.PowerState == PowerState.Charging
                    ? Semantics.StateCharging
                    : (status.BatteryPercent > 19 ? Semantics.StateActive : Semantics.StateWarning)
            };
        }

        public static Color GetStatusTitleColor(KeyboardStatus status)
        {
            return status.ConnectionState switch
            {
                ConnectionState.Sleeping => Semantics.StateSleeping,
                ConnectionState.Disconnected => Semantics.TextDisabled,
                _ => status.PowerState == PowerState.Charging
                    ? Semantics.StateCharging
                    : Semantics.StateBrand
            };
        }

        public static Color GetPillDotColor(KeyboardStatus status)
        {
            return status.ConnectionState switch
            {
                ConnectionState.Connected => Semantics.StateBrand,
                ConnectionState.Sleeping => Semantics.StateSleeping,
                _ => Semantics.StateWarning
            };
        }
    }
    #endregion
}
