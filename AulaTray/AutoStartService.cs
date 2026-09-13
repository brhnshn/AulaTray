using System;
using Microsoft.Win32;

namespace AulaTray;

/// <summary>
/// Windows başlangıç kayıtlarını yöneten servis.
/// Expand-Contract deseniyle 'AulaF75Tray' eski kaydını 'AulaTray' kaydına güvenle taşır.
/// </summary>
public static class AutoStartService
{
    public const string AppName = "AulaTray";
    public const string LegacyAppName = "AulaF75Tray";
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static bool IsAutoStartEnabled()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
            if (key == null) return false;

            // Genişlet (Expand): Hem güncel hem de eski anahtarı tanı
            return key.GetValue(AppName) != null || key.GetValue(LegacyAppName) != null;
        }
        catch
        {
            return false;
        }
    }

    public static void SetAutoStart(bool enable)
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
            if (key == null) return;

            if (enable)
            {
                string? exePath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(exePath))
                {
                    key.SetValue(AppName, $"\"{exePath}\"");
                    // Eski anahtarı temizle (Daralt / Contract)
                    key.DeleteValue(LegacyAppName, false);
                }
            }
            else
            {
                key.DeleteValue(AppName, false);
                key.DeleteValue(LegacyAppName, false);
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// Eski 'AulaF75Tray' kaydı mevcutsa yeni 'AulaTray' kaydına sessizce taşır.
    /// </summary>
    public static void MigrateLegacyRunKeyIfPresent()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
            if (key == null) return;

            object? legacyVal = key.GetValue(LegacyAppName);
            if (legacyVal != null)
            {
                string? exePath = Environment.ProcessPath;
                string targetPath = !string.IsNullOrEmpty(exePath) ? $"\"{exePath}\"" : legacyVal.ToString()!;
                key.SetValue(AppName, targetPath);
                key.DeleteValue(LegacyAppName, false);
            }
        }
        catch { }
    }
}
