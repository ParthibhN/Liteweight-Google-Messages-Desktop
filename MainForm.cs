using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace GoogleMessagesDesktop;

public partial class MainForm : Form
{
    private WebView2 _webView = null!;
    private NotifyIcon _trayIcon = null!;
    private ContextMenuStrip _trayMenu = null!;
    private bool _isExplicitExit = false;
    private bool _hasShownTrayNotification = false;

    private const string TargetUrl = "https://messages.google.com/web/";

    public MainForm()
    {
        InitializeComponent();
        InitializeTray();
        _ = InitializeWebViewAsync();
    }

    private void InitializeComponent()
    {
        Text = "Google Messages";
        Size = new Size(1150, 800);
        MinimumSize = new Size(650, 450);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = LoadApplicationIcon();

        _webView = new WebView2
        {
            Dock = DockStyle.Fill
        };

        Controls.Add(_webView);

        FormClosing += MainForm_FormClosing;
        Resize += MainForm_Resize;
    }

    private Icon LoadApplicationIcon()
    {
        // 1. Try extracting the embedded icon directly from the running .exe
        try
        {
            string? processPath = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(processPath) && File.Exists(processPath))
            {
                var exeIcon = Icon.ExtractAssociatedIcon(processPath);
                if (exeIcon != null)
                {
                    return exeIcon;
                }
            }
        }
        catch { }

        // 2. Try loading from a loose app.ico file in the directory
        try
        {
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
            if (File.Exists(iconPath))
            {
                return new Icon(iconPath);
            }
        }
        catch { }

        // 3. Fallback default system icon
        return SystemIcons.Application;
    }

    private void InitializeTray()
    {
        _trayMenu = new ContextMenuStrip();
        
        var openItem = new ToolStripMenuItem("Open Google Messages", null, (s, e) => RestoreWindow());
        openItem.Font = new Font(openItem.Font, FontStyle.Bold);
        
        var reloadItem = new ToolStripMenuItem("Reload Page", null, (s, e) => _webView.CoreWebView2?.Reload());
        var separator = new ToolStripSeparator();
        var exitItem = new ToolStripMenuItem("Exit", null, (s, e) => ExitApplication());

        _trayMenu.Items.AddRange(new ToolStripItem[] { openItem, reloadItem, separator, exitItem });

        _trayIcon = new NotifyIcon
        {
            Text = "Google Messages",
            Icon = Icon,
            ContextMenuStrip = _trayMenu,
            Visible = true
        };

        _trayIcon.DoubleClick += (s, e) => RestoreWindow();
    }

    private async Task InitializeWebViewAsync()
    {
        try
        {
            // Set dedicated user data folder to guarantee persistent login/session & cookies
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string userDataFolder = Path.Combine(appDataPath, "GoogleMessagesDesktop", "Profile");
            Directory.CreateDirectory(userDataFolder);

            var envOptions = new CoreWebView2EnvironmentOptions();
            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder, envOptions);

            await _webView.EnsureCoreWebView2Async(env);

            // Configure WebView2 settings
            _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            _webView.CoreWebView2.Settings.IsZoomControlEnabled = true;
            _webView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = true;

            // Use standard Chrome User Agent to avoid Google Auth / Pairing compatibility loops
            string currentUa = _webView.CoreWebView2.Settings.UserAgent;
            if (!string.IsNullOrEmpty(currentUa) && currentUa.Contains("Edg/"))
            {
                // Strip out Edg/ token so Google sees pure Chrome
                _webView.CoreWebView2.Settings.UserAgent = currentUa.Replace(System.Text.RegularExpressions.Regex.Match(currentUa, @"Edg/\S+").Value, "").Trim();
            }

            // Automatically grant required permissions (Notifications, Clipboard, Microphone)
            _webView.CoreWebView2.PermissionRequested += CoreWebView2_PermissionRequested;

            // Script bridge: Hook notifications & service worker conversation routing
            await _webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
                (function() {
                    // Hook Notification click events
                    if (window.Notification) {
                        const OriginalNotification = window.Notification;
                        window.Notification = function(title, options) {
                            const notif = new OriginalNotification(title, options);
                            notif.addEventListener('click', function() {
                                window.chrome?.webview?.postMessage('notification_clicked');
                            });
                            return notif;
                        };
                        window.Notification.permission = OriginalNotification.permission;
                        window.Notification.requestPermission = OriginalNotification.requestPermission.bind(OriginalNotification);
                    }

                    // Hook ServiceWorker notifications
                    if (navigator.serviceWorker) {
                        navigator.serviceWorker.addEventListener('message', function(e) {
                            window.chrome?.webview?.postMessage('notification_clicked');
                        });
                    }

                    // Detect URL navigation to a specific conversation
                    let lastUrl = location.href;
                    setInterval(function() {
                        if (location.href !== lastUrl) {
                            lastUrl = location.href;
                            if (location.href.includes('/conversations/')) {
                                window.chrome?.webview?.postMessage('conversation_opened');
                            }
                        }
                    }, 300);
                })();
            ");

            _webView.CoreWebView2.WebMessageReceived += (s, e) =>
            {
                string msg = e.TryGetWebMessageAsString();
                if (msg == "notification_clicked" || msg == "conversation_opened")
                {
                    this.BeginInvoke(RestoreWindow);
                }
            };

            // When user clicks a notification and URL changes to conversation thread, restore window if hidden
            _webView.CoreWebView2.SourceChanged += (s, e) =>
            {
                string? currentSource = _webView.CoreWebView2?.Source;
                if (!string.IsNullOrEmpty(currentSource) && currentSource.Contains("/conversations/"))
                {
                    if (!Visible || WindowState == FormWindowState.Minimized)
                    {
                        this.BeginInvoke(RestoreWindow);
                    }
                }
            };

            // Handle new window requests: keep Google Account/Auth flows internal, open external SMS links in system browser
            _webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;

            // Navigate to Google Messages Web
            _webView.CoreWebView2.Navigate(TargetUrl);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to initialize WebView2:\n{ex.Message}\n\nPlease ensure Microsoft Edge WebView2 Runtime is installed.",
                "Initialization Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void CoreWebView2_PermissionRequested(object? sender, CoreWebView2PermissionRequestedEventArgs e)
    {
        // Auto-allow notifications, audio/mic, and clipboard for Google Messages
        if (e.PermissionKind == CoreWebView2PermissionKind.Notifications ||
            e.PermissionKind == CoreWebView2PermissionKind.Microphone ||
            e.PermissionKind == CoreWebView2PermissionKind.ClipboardRead)
        {
            e.State = CoreWebView2PermissionState.Allow;
            e.Handled = true;
        }
    }

    private void CoreWebView2_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Uri)) return;

        // Keep all Google-related authentication, account pairing, and support URLs inside WebView2
        if (IsGoogleDomain(e.Uri))
        {
            // Allow popup or let WebView2 handle it internally
            return;
        }

        // External links clicked inside text messages open in the system's default browser
        e.Handled = true;
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open link: {ex.Message}");
        }
    }

    private static bool IsGoogleDomain(string uriString)
    {
        if (Uri.TryCreate(uriString, UriKind.Absolute, out Uri? uri))
        {
            string host = uri.Host.ToLowerInvariant();
            return host.EndsWith(".google.com") ||
                   host.EndsWith(".googleusercontent.com") ||
                   host.EndsWith(".googleapis.com") ||
                   host.EndsWith(".gstatic.com") ||
                   host == "google.com";
        }
        return false;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

    private FormWindowState _lastNonMinimizedState = FormWindowState.Normal;

    private void RestoreWindow()
    {
        if (!Visible)
        {
            Show();
        }

        if (WindowState == FormWindowState.Minimized)
        {
            WindowState = _lastNonMinimizedState;
        }

        // Force Windows to allow bringing window to the foreground on notification click
        keybd_event(0, 0, 0, 0);
        SetForegroundWindow(Handle);
        Activate();
        BringToFront();
    }

    private void ExitApplication()
    {
        _isExplicitExit = true;
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        Application.Exit();
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!_isExplicitExit && e.CloseReason == CloseReason.UserClosing)
        {
            // Minimize to tray on close
            e.Cancel = true;
            Hide();

            if (!_hasShownTrayNotification)
            {
                _trayIcon.ShowBalloonTip(
                    2500,
                    "Google Messages",
                    "Google Messages is running in the background. Right-click the tray icon to exit.",
                    ToolTipIcon.Info
                );
                _hasShownTrayNotification = true;
            }
        }
    }

    private void MainForm_Resize(object? sender, EventArgs e)
    {
        if (WindowState != FormWindowState.Minimized)
        {
            _lastNonMinimizedState = WindowState;
        }
        else
        {
            Hide();
        }
    }
}
