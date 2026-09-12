using System;
using System.Drawing;
using System.Windows.Forms;

namespace AulaTray;

public class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly System.Windows.Forms.Timer _pollingTimer;
    private readonly HidService _hidService;
    private readonly ToolStripMenuItem _statusMenuItem;
    private readonly ToolStripMenuItem _autoStartMenuItem;
    private bool _lowBatteryWarned = false;
    private Icon? _currentIcon = null;
    private StatusForm? _currentStatusForm = null;
    private KeyboardStatus _latestStatus = new KeyboardStatus(0, PowerState.Discharging, ConnectionState.Disconnected, ConnectionMode.Disconnected);

    public TrayApplicationContext()
    {
        _hidService = new HidService();

        ContextMenuStrip contextMenu = new ContextMenuStrip();
        
        ToolStripMenuItem showWindowItem = new ToolStripMenuItem("Aula F75 Durumu...", null, (_, _) => ShowStatusWindow())
        {
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };
        contextMenu.Items.Add(showWindowItem);

        _statusMenuItem = new ToolStripMenuItem("Algılanıyor...") { Enabled = false };
        contextMenu.Items.Add(_statusMenuItem);
        contextMenu.Items.Add(new ToolStripSeparator());

        ToolStripMenuItem refreshItem = new ToolStripMenuItem("Şimdi Yenile", null, (_, _) => RefreshStatus());
        contextMenu.Items.Add(refreshItem);

        bool autoStartEnabled = AutoStartService.IsAutoStartEnabled();
        _autoStartMenuItem = new ToolStripMenuItem("Windows ile Başlat", null, OnAutoStartToggle)
        {
            Checked = autoStartEnabled
        };
        contextMenu.Items.Add(_autoStartMenuItem);

        if (!autoStartEnabled)
        {
            AutoStartService.SetAutoStart(true);
            _autoStartMenuItem.Checked = true;
        }

        contextMenu.Items.Add(new ToolStripSeparator());

        ToolStripMenuItem exitItem = new ToolStripMenuItem("Çıkış", null, (_, _) => ExitApp());
        contextMenu.Items.Add(exitItem);

        _notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = contextMenu,
            Visible = true,
            Text = "Aula F75 Pil Monitörü"
        };
        
        _notifyIcon.MouseClick += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowStatusWindow();
            }
        };
        _notifyIcon.DoubleClick += (_, _) => ShowStatusWindow();

        _pollingTimer = new System.Windows.Forms.Timer
        {
            Interval = 15000 // 15 saniyede bir yokla
        };
        _pollingTimer.Tick += (_, _) => RefreshStatus();
        _pollingTimer.Start();

        RefreshStatus();
    }

    private void ShowStatusWindow()
    {
        if (_currentStatusForm != null && !_currentStatusForm.IsDisposed)
        {
            _currentStatusForm.Close();
            _currentStatusForm = null;
            return;
        }

        _latestStatus = _hidService.QueryStatus();
        _currentStatusForm = new StatusForm(_latestStatus);
        _currentStatusForm.FormClosed += (_, _) => _currentStatusForm = null;
        _currentStatusForm.Show();
    }

    private void RefreshStatus()
    {
        _latestStatus = _hidService.QueryStatus();
        UpdateUI(_latestStatus);
    }

    private void UpdateUI(KeyboardStatus status)
    {
        Icon newIcon = IconRenderer.CreateBatteryIcon(status);
        Icon? oldIcon = _currentIcon;
        _currentIcon = newIcon;
        _notifyIcon.Icon = newIcon;

        if (oldIcon != null)
        {
            DestroyIcon(oldIcon.Handle);
            oldIcon.Dispose();
        }

        string statusText;
        if (status.ConnectionState == ConnectionState.Disconnected)
        {
            statusText = "Aula F75: Bağlantı Yok";
            _statusMenuItem.Text = statusText;
            _notifyIcon.Text = TruncateTooltip(statusText);
            return;
        }

        if (status.ConnectionState == ConnectionState.Sleeping)
        {
            statusText = "Aula F75: Uykuda";
            _statusMenuItem.Text = statusText;
            _notifyIcon.Text = TruncateTooltip(statusText);
            return;
        }

        string modeShort = status.Mode switch
        {
            ConnectionMode.Wired => "Kablolu",
            ConnectionMode.Wireless24G => "2.4G",
            ConnectionMode.Bluetooth => "Bluetooth",
            _ => ""
        };

        string powerText = status.PowerState == PowerState.Charging ? "Şarj Oluyor" : "Pilde";
        statusText = $"Aula F75: %{status.BatteryPercent} ({modeShort} - {powerText})";
        
        _statusMenuItem.Text = statusText;
        _notifyIcon.Text = TruncateTooltip(statusText);

        if (status.BatteryPercent <= 20 && status.PowerState == PowerState.Discharging)
        {
            if (!_lowBatteryWarned)
            {
                _notifyIcon.ShowBalloonTip(
                    3000,
                    "Aula F75 Pil Uyarısı",
                    $"Pil seviyesi %{status.BatteryPercent}. Lütfen klavyenizi şarja takın.",
                    ToolTipIcon.Warning
                );
                _lowBatteryWarned = true;
            }
        }
        else if (status.BatteryPercent > 25 || status.PowerState == PowerState.Charging)
        {
            _lowBatteryWarned = false;
        }
    }

    private static string TruncateTooltip(string text)
    {
        return text.Length > 63 ? text.Substring(0, 60) + "..." : text;
    }

    private void OnAutoStartToggle(object? sender, EventArgs e)
    {
        bool newState = !_autoStartMenuItem.Checked;
        AutoStartService.SetAutoStart(newState);
        _autoStartMenuItem.Checked = newState;
    }

    private void ExitApp()
    {
        _pollingTimer.Stop();
        _pollingTimer.Dispose();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _currentStatusForm?.Close();
        if (_currentIcon != null)
        {
            DestroyIcon(_currentIcon.Handle);
            _currentIcon.Dispose();
        }
        ExitThread();
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern bool DestroyIcon(IntPtr handle);
}
