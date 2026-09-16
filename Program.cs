using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace GoogleMessagesDesktop;

internal static class Program
{
    private const string AppGuid = "GoogleMessagesDesktop_8F4C8C21-6893-47E4-B27D-5A8E1D9F10B2";

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

    private const int SW_RESTORE = 9;

    [STAThread]
    private static void Main()
    {
        using var mutex = new Mutex(true, AppGuid, out bool isFirstInstance);

        if (!isFirstInstance)
        {
            // Another instance is already running; bring it to foreground
            IntPtr hWnd = FindWindow(null, "Google Messages");
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, SW_RESTORE);
                SetForegroundWindow(hWnd);
            }
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
