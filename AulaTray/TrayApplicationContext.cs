using System;
using System.Drawing;
using System.Windows.Forms;

namespace AulaTray;

public class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly KeyboardMonitor _keyboardMonitor;
    private readonly System.Threading.SynchronizationContext? _syncContext;
    private readonly ToolStripMenuItem _showWindowItem;
    private readonly ToolStripMenuItem _statusMenuItem;
    private readonly ToolStripMenuItem _autoStartMenuItem;
    private bool _lowBatteryWarned = false;
    private StatusForm? _currentStatusForm = null;
    private KeyboardStatus _latestStatus = new(0, PowerState.Discharging, ConnectionState.Disconnected, ConnectionMode.Disconnected);

    public TrayApplicationContext()
    {
        _syncContext = System.Threading.SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
        _keyboardMonitor = new KeyboardMonitor();

        ContextMenuStrip contextMenu = new ContextMenuStrip();
        
        _showWindowItem = new ToolStripMenuItem("Aula Klavye Durumu...", null, (_, _) => ShowStatusWindow())
        {
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };
        contextMenu.Items.Add(_showWindowItem);

        _statusMenuItem = new ToolStripMenuItem("Algılanıyor...") { Enabled = false };
        contextMenu.Items.Add(_statusMenuItem);
        contextMenu.Items.Add(new ToolStripSeparator());

        ToolStripMenuItem refreshItem = new ToolStripMenuItem("Şimdi Yenile", null, (_, _) => RefreshStatus());
        contextMenu.Items.Add(refreshItem);

        AutoStartService.MigrateLegacyRunKeyIfPresent();
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
            Text = "Aula Klavye Pil Monitörü"
        };
        
        _notifyIcon.MouseClick += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowStatusWindow();
            }
        };
        _notifyIcon.DoubleClick += (_, _) => ShowStatusWindow();

        _keyboardMonitor.StatusChanged += status =>
        {
            if (_syncContext != null)
                _syncContext.Post(_ => UpdateUI(status), null);
            else
                UpdateUI(status);
        };
        _keyboardMonitor.Start();

        // Başlangıç JIT ve CLR yükünden sonra RAM kullanımını 70MB'tan 10-15MB'a daralt
        System.Threading.Tasks.Task.Delay(2500).ContinueWith(_ => MemoryOptimizer.TrimWorkingSet());
    }

    private void ShowStatusWindow()
    {
        if (_currentStatusForm != null && !_currentStatusForm.IsDisposed)
        {
            _currentStatusForm.Close();
            _currentStatusForm = null;
            return;
        }

        _latestStatus = _keyboardMonitor.LatestStatus;
        _currentStatusForm = new StatusForm(_latestStatus);
        _currentStatusForm.FormClosed += (_, _) => _currentStatusForm = null;
        _currentStatusForm.Show();
    }

    private void RefreshStatus()
    {
        _keyboardMonitor.QueryNow();
    }

    private void UpdateUI(KeyboardStatus status)
    {
        _currentStatusForm?.UpdateStatus(status);

        Icon targetIcon = IconRenderer.GetTrayIcon(status);
        if (_notifyIcon.Icon != targetIcon)
        {
            _notifyIcon.Icon = targetIcon;
        }

        string modelName = !string.IsNullOrWhiteSpace(status.ModelName) ? status.ModelName : "Aula Klavye";
        _showWindowItem.Text = $"{modelName} Durumu...";

        string statusText;
        if (status.ConnectionState == ConnectionState.Disconnected)
        {
            statusText = $"{modelName}: Bağlantı Yok";
            _statusMenuItem.Text = statusText;
            _notifyIcon.Text = TruncateTooltip(statusText);
            return;
        }

        if (status.ConnectionState == ConnectionState.Sleeping)
        {
            statusText = $"{modelName}: %{status.BatteryPercent} (Uykuda)";
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
        statusText = $"{modelName}: %{status.BatteryPercent} ({modeShort} - {powerText})";
        
        _statusMenuItem.Text = statusText;
        _notifyIcon.Text = TruncateTooltip(statusText);

        if (status.BatteryPercent <= 20 && status.PowerState == PowerState.Discharging)
        {
            if (!_lowBatteryWarned)
            {
                _notifyIcon.ShowBalloonTip(
                    3000,
                    $"{modelName} Pil Uyarısı",
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
        _keyboardMonitor.Dispose();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _currentStatusForm?.Close();
        IconRenderer.CleanUp();
        ExitThread();
    }
}
