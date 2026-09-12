using System;
using System.IO;
using System.Windows.Forms;

namespace AulaTray;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new TrayApplicationContext());
        }
        catch (Exception ex)
        {
            File.WriteAllText("crash.log", ex.ToString());
        }
    }
}
