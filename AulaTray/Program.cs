using System;
using System.IO;
using System.Windows.Forms;

namespace AulaTray;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            RecordCrash(e.ExceptionObject?.ToString());
        };

        Application.ThreadException += (s, e) =>
        {
            RecordCrash(e.Exception?.ToString());
        };

        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new TrayApplicationContext());
        }
        catch (Exception ex)
        {
            RecordCrash(ex.ToString());
        }
    }

    private static void RecordCrash(string? error)
    {
        if (string.IsNullOrEmpty(error)) return;
        try
        {
            string crashPath = Path.Combine(AppContext.BaseDirectory, "crash.log");
            File.WriteAllText(crashPath, error);
        }
        catch { }
    }
}
