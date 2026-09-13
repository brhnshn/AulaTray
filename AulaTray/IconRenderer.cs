using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace AulaTray;

/// <summary>
/// Sistem tepsisi ikonlarını yöneten ve sıfır bellek tahsisi (zero-allocation) ile
/// önbellekleyen derin modül servisi.
/// </summary>
public static class IconRenderer
{
    private static readonly Image? _baseLogo;
    private static readonly Icon _activeIcon;
    private static readonly Icon _dimmedIcon;

    private static readonly IntPtr _hActiveIcon = IntPtr.Zero;
    private static readonly IntPtr _hDimmedIcon = IntPtr.Zero;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool DestroyIcon(IntPtr handle);

    static IconRenderer()
    {
        try
        {
            using var resStream = typeof(IconRenderer).Assembly.GetManifestResourceStream("AulaTray.AulaLogo.png");
            if (resStream != null)
            {
                using var tempBmp = new Bitmap(resStream);
                _baseLogo = new Bitmap(tempBmp);
            }
            else
            {
                string logoPath = Path.Combine(AppContext.BaseDirectory, "AulaLogo.png");
                if (File.Exists(logoPath))
                {
                    using var tempBmp = new Bitmap(logoPath);
                    _baseLogo = new Bitmap(tempBmp);
                }
            }
        }
        catch { }

        // 1. Aktif (Canlı) İkonu Tek Seferde Üret ve Önbellekle
        using (Bitmap activeBmp = RenderIconBitmap(isActive: true))
        {
            _hActiveIcon = activeBmp.GetHicon();
            _activeIcon = Icon.FromHandle(_hActiveIcon);
        }

        // 2. Loş (Uykuda / Bağlantı Yok) İkonu Tek Seferde Üret ve Önbellekle
        using (Bitmap dimmedBmp = RenderIconBitmap(isActive: false))
        {
            _hDimmedIcon = dimmedBmp.GetHicon();
            _dimmedIcon = Icon.FromHandle(_hDimmedIcon);
        }
    }

    private static Bitmap RenderIconBitmap(bool isActive)
    {
        Bitmap bitmap = new Bitmap(32, 32);
        using Graphics g = Graphics.FromImage(bitmap);

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(Color.Transparent);

        if (_baseLogo != null)
        {
            if (isActive)
            {
                // Bağlıyken: Canlı, parlak metalik cam A logosu
                g.DrawImage(_baseLogo, new Rectangle(1, 1, 30, 30));
            }
            else
            {
                // Uykuda veya Bağlantı Yokken: Loş, yarı saydam ve zarif gri tonlamalı A logosu
                ColorMatrix cm = new ColorMatrix(new float[][] {
                    new float[] {0.35f, 0.35f, 0.35f, 0, 0},
                    new float[] {0.35f, 0.35f, 0.35f, 0, 0},
                    new float[] {0.35f, 0.35f, 0.35f, 0, 0},
                    new float[] {0, 0, 0, 0.40f, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

                using ImageAttributes ia = new ImageAttributes();
                ia.SetColorMatrix(cm);
                g.DrawImage(_baseLogo, new Rectangle(1, 1, 30, 30), 0, 0, _baseLogo.Width, _baseLogo.Height, GraphicsUnit.Pixel, ia);
            }
        }
        else
        {
            // Fallback (Logo yüklenemezse sade 'A' harfi)
            using Font font = new Font(ThemeTokens.Primitives.FontFamilySans, 16f, FontStyle.Bold);
            using Brush brush = new SolidBrush(isActive ? Color.White : Color.Gray);
            g.DrawString("A", font, brush, 5, 2);
        }

        return bitmap;
    }

    /// <summary>
    /// O(1) hızında, sıfır bellek tahsisiyle durumuna uygun önbelleklenmiş ikonu döner.
    /// </summary>
    public static Icon GetTrayIcon(KeyboardStatus status)
    {
        return status.ConnectionState == ConnectionState.Connected
            ? _activeIcon
            : _dimmedIcon;
    }

    /// <summary>
    /// Geriye uyumluluk için GetTrayIcon sonucunu döndürür (Kullanımdan kaldırılmıştır).
    /// </summary>
    [Obsolete("Bu metot kullanımdan kaldırılmıştır. Sıfır tahsisli GetTrayIcon(status) kullanın.")]
    public static Icon CreateBatteryIcon(KeyboardStatus status) => GetTrayIcon(status);

    /// <summary>
    /// Uygulama kapanırken unmanaged GDI tanıtıcılarını serbest bırakır.
    /// </summary>
    public static void CleanUp()
    {
        try
        {
            if (_hActiveIcon != IntPtr.Zero) DestroyIcon(_hActiveIcon);
            if (_hDimmedIcon != IntPtr.Zero) DestroyIcon(_hDimmedIcon);
            _activeIcon?.Dispose();
            _dimmedIcon?.Dispose();
            _baseLogo?.Dispose();
        }
        catch { }
    }
}
