using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AulaTray;

public class StatusForm : Form
{
    private readonly KeyboardStatus _status;
    private Image? _badgeImage;

    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_MBUTTONDOWN = 0x0207;
    private const int WM_NCLBUTTONDOWN = 0x00A1;
    private const int WM_NCRBUTTONDOWN = 0x00A4;

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    private LowLevelMouseProc? _mouseProc;
    private IntPtr _hookId = IntPtr.Zero;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    public StatusForm(KeyboardStatus status)
    {
        _status = status;

        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Size = new Size(310, 180);
        BackColor = Color.FromArgb(20, 22, 28);
        ShowInTaskbar = false;
        TopMost = true;

        Rectangle workingArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);
        Location = new Point(workingArea.Right - Width - 16, workingArea.Bottom - Height - 16);

        try
        {
            var resStream = typeof(StatusForm).Assembly.GetManifestResourceStream("AulaTray.aulatray_badge.png");
            if (resStream != null)
            {
                _badgeImage = Image.FromStream(resStream);
            }
            else
            {
                string badgePath = Path.Combine(AppContext.BaseDirectory, "aulatray_badge.png");
                if (File.Exists(badgePath))
                {
                    _badgeImage = Image.FromFile(badgePath);
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
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        // Dış Çerçeve
        using (Pen borderPen = new Pen(Color.FromArgb(45, 50, 65), 1.5f))
        {
            DrawRoundedRectangle(g, borderPen, 1, 1, Width - 2, Height - 2, 10f);
        }

        // Üst Başlık (Masaüstündeki Şık AulaTray Cam Rozeti)
        if (_badgeImage != null)
        {
            g.DrawImage(_badgeImage, new Rectangle(18, 10, 86, 42));
        }
        else
        {
            using (Font titleFont = new Font("Segoe UI", 11f, FontStyle.Bold))
            using (Brush titleBrush = new SolidBrush(Color.White))
            {
                g.DrawString("AulaTray", titleFont, titleBrush, 20, 16);
            }
        }

        using (Font subFont = new Font("Segoe UI", 8.5f, FontStyle.Regular))
        using (Brush subBrush = new SolidBrush(Color.FromArgb(120, 140, 165)))
        {
            g.DrawString("Aula F75", subFont, subBrush, Width - 65, 22);
        }

        // Ayırıcı ince çizgi
        using (Pen sepPen = new Pen(Color.FromArgb(35, 40, 52), 1f))
        {
            g.DrawLine(sepPen, 20, 56, Width - 20, 56);
        }

        // Batarya Yüzdesi
        int batY = 66;
        Color neonColor = _status.PowerState == PowerState.Charging 
            ? Color.FromArgb(0, 220, 255) 
            : (_status.BatteryPercent > 45 ? Color.FromArgb(0, 245, 160) : (_status.BatteryPercent > 19 ? Color.FromArgb(255, 205, 50) : Color.FromArgb(255, 75, 75)));

        string percentStr = _status.ConnectionState == ConnectionState.Connected ? $"%{_status.BatteryPercent}" : "--";
        using (Font percentFont = new Font("Segoe UI", 24f, FontStyle.Bold))
        using (Brush percentBrush = new SolidBrush(Color.White))
        {
            g.DrawString(percentStr, percentFont, percentBrush, 20, batY);
        }

        string statusDesc;
        if (_status.ConnectionState == ConnectionState.Disconnected)
            statusDesc = "Alıcı / Bağlantı Yok";
        else if (_status.ConnectionState == ConnectionState.Sleeping)
            statusDesc = "Uykuda";
        else if (_status.PowerState == PowerState.Charging)
            statusDesc = "Şarj Ediliyor";
        else
            statusDesc = "Pilde Çalışıyor";

        using (Font descFont = new Font("Segoe UI", 9.5f, FontStyle.Regular))
        using (Brush descBrush = new SolidBrush(neonColor))
        {
            g.DrawString(statusDesc, descFont, descBrush, 120, batY + 4);
        }

        string modeDesc = _status.Mode switch
        {
            ConnectionMode.Wired => "USB Kablolu Modu",
            ConnectionMode.Wireless24G => "2.4 GHz Kablosuz Modu",
            ConnectionMode.Bluetooth => "Bluetooth 5.0 Modu",
            _ => "Bağlantı Yok"
        };

        using (Font modeFont = new Font("Segoe UI", 8.5f, FontStyle.Regular))
        using (Brush modeBrush = new SolidBrush(Color.FromArgb(140, 145, 160)))
        {
            g.DrawString(modeDesc, modeFont, modeBrush, 120, batY + 22);
        }

        // Neon Doluluk Barı
        int barY = 116;
        int barW = Width - 40;
        int barH = 7;
        using (GraphicsPath barBgPath = CreateRoundedRectPath(20, barY, barW, barH, 3.5f))
        using (Brush barBg = new SolidBrush(Color.FromArgb(35, 40, 52)))
        {
            g.FillPath(barBg, barBgPath);
        }

        if (_status.ConnectionState == ConnectionState.Connected && _status.BatteryPercent > 0)
        {
            float fillRatio = Math.Clamp(_status.BatteryPercent / 100f, 0.05f, 1.0f);
            float fillW = barW * fillRatio;
            using (GraphicsPath fillPath = CreateRoundedRectPath(20, barY, fillW, barH, 3.5f))
            using (Brush fillBrush = new SolidBrush(neonColor))
            {
                g.FillPath(fillBrush, fillPath);
            }
        }

        // Tek Sade Bilgi Satırı (Aygıt)
        int infoY = 138;
        string deviceName = _status.Mode switch
        {
            ConnectionMode.Wired => "AULA F75 (Kablolu USB)",
            ConnectionMode.Wireless24G => "Compx 2.4G Alıcı (0x3554)",
            ConnectionMode.Bluetooth => "AULA-F75 5.0 KB (Bluetooth)",
            _ => "Aygıt Bulunamadı"
        };

        DrawInfoRow(g, 20, infoY, Width - 40, "Aygıt", deviceName);
    }

    private void DrawInfoRow(Graphics g, int x, int y, int w, string label, string value)
    {
        using Font labelFont = new Font("Segoe UI", 8.5f, FontStyle.Regular);
        using Brush labelBrush = new SolidBrush(Color.FromArgb(130, 135, 150));
        g.DrawString(label, labelFont, labelBrush, x, y);

        using Font valFont = new Font("Segoe UI", 8.5f, FontStyle.Regular);
        using Brush valBrush = new SolidBrush(Color.FromArgb(210, 215, 230));
        SizeF valSize = g.MeasureString(value, valFont);
        g.DrawString(value, valFont, valBrush, x + w - valSize.Width, y);
    }

    private static void DrawRoundedRectangle(Graphics g, Pen pen, float x, float y, float width, float height, float radius)
    {
        using GraphicsPath path = CreateRoundedRectPath(x, y, width, height, radius);
        g.DrawPath(pen, path);
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
        InstallMouseHook();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        UnhookMouse();
        base.OnFormClosed(e);
    }

    private void InstallMouseHook()
    {
        if (_hookId == IntPtr.Zero)
        {
            _mouseProc = HookCallback;
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            IntPtr moduleHandle = curModule != null ? GetModuleHandle(curModule.ModuleName) : IntPtr.Zero;
            _hookId = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, moduleHandle, 0);
        }
    }

    private void UnhookMouse()
    {
        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
            _mouseProc = null;
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int msg = (int)wParam;
            if (msg == WM_LBUTTONDOWN || msg == WM_RBUTTONDOWN || msg == WM_MBUTTONDOWN ||
                msg == WM_NCLBUTTONDOWN || msg == WM_NCRBUTTONDOWN)
            {
                MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                Point clickPt = new Point(hookStruct.pt.x, hookStruct.pt.y);
                if (!this.Bounds.Contains(clickPt))
                {
                    BeginInvoke(new Action(Close));
                }
            }
        }
        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            UnhookMouse();
            _badgeImage?.Dispose();
        }
        base.Dispose(disposing);
    }
}
