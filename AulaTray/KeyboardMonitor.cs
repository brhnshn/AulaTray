using System;
using System.Threading;
using System.Threading.Tasks;

namespace AulaTray;

/// <summary>
/// UI iş parçacığından tamamen bağımsız çalışan, klavye bağlantı ve pil durumunu
/// arka planda periyodik olarak sorgulayıp değişiklikleri olayla (event) bildiren
/// asenkron izleme motoru (Background Monitor).
/// </summary>
public class KeyboardMonitor : IDisposable
{
    private readonly HidService _hidService;
    private readonly int _intervalMs;
    private CancellationTokenSource? _cts;
    private Task? _monitorTask;
    private bool _disposed = false;

    public event Action<KeyboardStatus>? StatusChanged;

    public KeyboardStatus LatestStatus { get; private set; } = new(0, PowerState.Discharging, ConnectionState.Disconnected, ConnectionMode.Disconnected);

    public KeyboardMonitor(HidService? hidService = null, int intervalMs = 3500)
    {
        _hidService = hidService ?? new HidService();
        _intervalMs = intervalMs;
    }

    public void Start()
    {
        if (_monitorTask != null && !_monitorTask.IsCompleted) return;

        _cts = new CancellationTokenSource();
        _monitorTask = Task.Run(() => WorkerLoopAsync(_cts.Token));
    }

    public void Stop()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    public KeyboardStatus QueryNow()
    {
        var status = _hidService.QueryStatus();
        LatestStatus = status;
        StatusChanged?.Invoke(status);
        return status;
    }

    private async Task WorkerLoopAsync(CancellationToken ct)
    {
        // İlk sorguyu hemen çalıştır
        try
        {
            LatestStatus = _hidService.QueryStatus();
            StatusChanged?.Invoke(LatestStatus);
        }
        catch { }

        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_intervalMs));

        while (!ct.IsCancellationRequested)
        {
            try
            {
                if (!await timer.WaitForNextTickAsync(ct))
                    break;

                var status = _hidService.QueryStatus();
                LatestStatus = status;
                StatusChanged?.Invoke(status);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // Arka plan sorgu hataları döngüyü kırmaz
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            Stop();
        }
    }
}
