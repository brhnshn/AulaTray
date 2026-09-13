using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AulaTray;

public class StatusForm : Form
{
    private KeyboardStatus _status;
    private Image? _logoImage;

    // Önbelleklenmiş GDI Nesneleri (Sıfır Tahsisli Çizim)
    private static readonly Font _fontTitle = new(ThemeTokens.Primitives.FontFamilySans, 9.5f, FontStyle.Bold);
    private static readonly Font _fontSub = new(ThemeTokens.Primitives.FontFamilySans, 8f, FontStyle.Regular);
    private static readonly Font _fontMode = new(ThemeTokens.Primitives.FontFamilyMono, 8.5f, FontStyle.Regular);
    private static readonly Font _fontNum = new(ThemeTokens.Primitives.FontFamilyMono, 15f, FontStyle.Bold);
    private static readonly Font _fontStatusTitle = new(ThemeTokens.Primitives.FontFamilySans, 9.8f, FontStyle.Bold);
    private static readonly Font _fontStatusDesc = new(ThemeTokens.Primitives.FontFamilySans, 8.2f, FontStyle.Regular);

    private static readonly Brush _brushSurfaceBase = new SolidBrush(ThemeTokens.Semantics.SurfaceBase);
    private static readonly Pen _penBorderSubtle = new(ThemeTokens.Semantics.BorderSubtle, 1f);
    private static readonly Brush _brushBrandBoxBg = new SolidBrush(ThemeTokens.Components.BrandBoxBg);
    private static readonly Brush _brushTextPrimary = new SolidBrush(ThemeTokens.Semantics.TextPrimary);
    private static readonly Brush _brushTextMuted = new SolidBrush(ThemeTokens.Semantics.TextMuted);
    private static readonly Brush _brushPillBg = new SolidBrush(ThemeTokens.Components.PillBackground);
    private static readonly Pen _penPillBorder = new(ThemeTokens.Components.PillBorderColor, 1f);
    private static readonly Brush _brushPillText = new SolidBrush(ThemeTokens.Components.PillTextColor);
    private static readonly Brush _brushSurfaceCard = new SolidBrush(ThemeTokens.Semantics.SurfaceCard);
    private static readonly Pen _penRingTrack = new(ThemeTokens.Components.RingTrackColor, ThemeTokens.Components.RingStrokeWidth);

    public void UpdateStatus(KeyboardStatus status)
    {
        _status = status;
        Invalidate();
    }

    private const int WM_ACTIVATE = 0x0006;
    private const int WA_INACTIVE = 0;

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    public StatusForm(KeyboardStatus status)
    {
        _status = status;

        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Size = ThemeTokens.Components.WindowSize;
        BackColor = ThemeTokens.Components.WindowBackground;
        ShowInTaskbar = false;
        TopMost = true;

        Rectangle workingArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);
        Location = new Point(workingArea.Right - Width - 16, workingArea.Bottom - Height - 16);

        try
        {
            using var resStream = typeof(StatusForm).Assembly.GetManifestResourceStream("AulaTray.AulaLogo.png");
            if (resStream != null)
            {
                using var tempBmp = new Bitmap(resStream);
                _logoImage = new Bitmap(tempBmp);
            }
            else
            {
                string logoPath = Path.Combine(AppContext.BaseDirectory, "AulaLogo.png");
                if (File.Exists(logoPath))
                {
                    using var tempBmp = new Bitmap(logoPath);
                    _logoImage = new Bitmap(tempBmp);
                }
            }
        }
        catch { }

        KeyPreview = true;
        KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };
        Deactivate += (s, e) => Close();

        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        // 1. Ana Kart Arka Planı ve Kenarlığı
        using (GraphicsPath mainBgPath = CreateRoundedRectPath(0, 0, Width, Height, 14f))
        {
            g.FillPath(_brushSurfaceBase, mainBgPath);
        }

        using (GraphicsPath mainBorderPath = CreateRoundedRectPath(0.5f, 0.5f, Width - 1f, Height - 1f, 14f))
        {
            g.DrawPath(_penBorderSubtle, mainBorderPath);
        }

        // 2. Header Bölümü
        int hY = 18;
        int iconBoxX = 18;
        int iconBoxSize = 30;

        // Sol İkon Kutusu
        using (GraphicsPath iconBoxPath = CreateRoundedRectPath(iconBoxX, hY, iconBoxSize, iconBoxSize, ThemeTokens.Components.BrandBoxRadius))
        {
            g.FillPath(_brushBrandBoxBg, iconBoxPath);
        }

        if (_logoImage != null)
        {
            g.DrawImage(_logoImage, new Rectangle(iconBoxX + 4, hY + 4, 22, 22));
        }
        else
        {
            DrawKeyboardIcon(g, iconBoxX + 6, hY + 8, 18, 14, ThemeTokens.Semantics.StateBrand);
        }

        // Başlık ve Alt Başlık (Model Adı Dinamik)
        g.DrawString("AulaTray", _fontTitle, _brushTextPrimary, iconBoxX + iconBoxSize + 10, hY);
        g.DrawString(_status.ModelName, _fontSub, _brushTextMuted, iconBoxX + iconBoxSize + 10, hY + 15);

        // Sağ Üst Mod Kapsülü
        string modeText = _status.ConnectionState == ConnectionState.Disconnected
            ? "Yok"
            : _status.Mode switch
            {
                ConnectionMode.Wired => "USB",
                ConnectionMode.Wireless24G => "2.4G",
                ConnectionMode.Bluetooth => "BT",
                _ => "Yok"
            };

        SizeF modeTextSize = g.MeasureString(modeText, _fontMode);
        int pillW = (int)modeTextSize.Width + 22;
        int pillH = 22;
        int pillX = Width - 18 - pillW;

        using (GraphicsPath pillPath = CreateRoundedRectPath(pillX, hY + 4, pillW, pillH, ThemeTokens.Components.PillRadius))
        {
            g.FillPath(_brushPillBg, pillPath);
            g.DrawPath(_penPillBorder, pillPath);
        }

        // Kapsül içindeki durum noktası
        Color dotColor = ThemeTokens.Components.GetPillDotColor(_status);
        using (Brush dotBrush = new SolidBrush(dotColor))
        {
            g.FillEllipse(dotBrush, pillX + 6, hY + 4 + 8, 6, 6);
        }

        g.DrawString(modeText, _fontMode, _brushPillText, pillX + 16, hY + 4 + 3);

        // 3. Orta Gövde Kartı
        int bodyX = 18;
        int bodyY = 64;
        int bodyW = Width - 36;
        int bodyH = 108;

        using (GraphicsPath bodyPath = CreateRoundedRectPath(bodyX, bodyY, bodyW, bodyH, 10f))
        {
            g.FillPath(_brushSurfaceCard, bodyPath);
        }

        // 4. Dairesel Batarya Halkası
        int ringSize = 72;
        int ringX = bodyX + 16;
        int ringY = bodyY + (bodyH - ringSize) / 2;
        float strokeWidth = ThemeTokens.Components.RingStrokeWidth;

        RectangleF ringRect = new RectangleF(ringX + strokeWidth / 2f, ringY + strokeWidth / 2f, ringSize - strokeWidth, ringSize - strokeWidth);

        // Arka Plan Çemberi
        g.DrawEllipse(_penRingTrack, ringRect);

        // Renk Belirleme (Tokens)
        Color ringColor = ThemeTokens.Components.GetRingProgressColor(_status);

        // Doluluk Yayı (Sweep Angle)
        if (_status.ConnectionState != ConnectionState.Disconnected && _status.BatteryPercent > 0)
        {
            float sweepAngle = (_status.BatteryPercent / 100f) * 360f;
            using (Pen ringProgPen = new Pen(ringColor, strokeWidth))
            {
                ringProgPen.StartCap = LineCap.Round;
                ringProgPen.EndCap = LineCap.Round;
                g.DrawArc(ringProgPen, ringRect, -90f, Math.Max(sweepAngle, 2f));
            }
        }

        // Merkez Sayı (Consolas / Monospace)
        string numStr = _status.ConnectionState == ConnectionState.Disconnected ? "--" : _status.BatteryPercent.ToString();
        SizeF numSize = g.MeasureString(numStr, _fontNum);
        float numX = ringX + (ringSize - numSize.Width) / 2f;
        float numY = ringY + (ringSize - numSize.Height) / 2f;
        g.DrawString(numStr, _fontNum, _brushTextPrimary, numX, numY);

        // 5. Durum Metinleri (Sağ Taraf)
        int textX = ringX + ringSize + 16;
        int textY = bodyY + 34;

        string titleText = _status.ConnectionState switch
        {
            ConnectionState.Disconnected => "Çevrimdışı",
            ConnectionState.Sleeping => "Uykuda",
            _ => _status.PowerState == PowerState.Charging ? "Şarj oluyor" : "Pilde çalışıyor"
        };

        string descText = _status.ConnectionState switch
        {
            ConnectionState.Disconnected => "Bağlantı yok",
            ConnectionState.Sleeping => "Tuşa basınca uyanır",
            _ => _status.PowerState == PowerState.Charging
                ? "Kabloyla şarj ediliyor"
                : (_status.Mode switch
                {
                    ConnectionMode.Wired => "Kablolu bağlantı",
                    ConnectionMode.Bluetooth => "Bluetooth bağlantısı",
                    _ => "Aktif kablosuz bağlantı"
                })
        };

        Color titleColor = ThemeTokens.Components.GetStatusTitleColor(_status);
        using (Brush statusTitleBrush = new SolidBrush(titleColor))
        {
            g.DrawString(titleText, _fontStatusTitle, statusTitleBrush, textX, textY);
        }

        g.DrawString(descText, _fontStatusDesc, _brushTextMuted, textX, textY + 20);
    }

    private void DrawKeyboardIcon(Graphics g, int x, int y, int w, int h, Color color)
    {
        using GraphicsPath kbPath = CreateRoundedRectPath(x, y, w, h, 2.5f);
        using Pen kbPen = new Pen(color, 1.2f);
        g.DrawPath(kbPen, kbPath);

        using Brush b = new SolidBrush(color);
        g.FillRectangle(b, x + 3, y + 3, 2, 2);
        g.FillRectangle(b, x + 7, y + 3, 2, 2);
        g.FillRectangle(b, x + 11, y + 3, 2, 2);
        g.FillRectangle(b, x + 4, y + 7, 8, 2);
    }

    private static GraphicsPath CreateRoundedRectPath(float x, float y, float w, float h, float r)
    {
        GraphicsPath path = new GraphicsPath();
        if (r <= 0) { path.AddRectangle(new RectangleF(x, y, w, h)); return path; }
        float d = r * 2;
        if (d > w) d = w;
        if (d > h) d = h;

        path.AddArc(x, y, d, d, 180, 90);
        path.AddArc(x + w - d, y, d, d, 270, 90);
        path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
        path.AddArc(x, y + h - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        SetForegroundWindow(Handle);
        Activate();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        // Form kapandığında geçici grafik nesnelerini temizle ve RAM'i daralt
        MemoryOptimizer.TrimWorkingSet();
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_ACTIVATE && (int)m.WParam == WA_INACTIVE)
        {
            Close();
            return;
        }
        base.WndProc(ref m);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _logoImage?.Dispose();
        }
        base.Dispose(disposing);
    }
}
