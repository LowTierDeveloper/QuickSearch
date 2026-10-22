
#nullable enable

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace QuickSearch;

public partial class Form1 : Form
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(
        IntPtr hWnd, int id, uint modifiers, uint key);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(
        IntPtr hWnd, int id);

    private const int ToggleHotkey = 1;
    private const int ExitHotkey = 2;
    private const int WmHotkey = 0x0312;

    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint VkSpace = 0x20;
    private const uint VkQ = 0x51;

    private readonly TextBox searchBox = new();
    private readonly WebView2 webView = new();
    private readonly Button backButton = new();
    private readonly Button forwardButton = new();
    private readonly Button refreshButton = new();
    private readonly Button searchButton = new();
    private readonly Button chromeButton = new();
    private readonly Label statusLabel = new();
    private readonly Label taglineLabel;

    private readonly bool startInBackground;

    private bool browserReady;
    private bool resultsVisible;
    private bool toggleRegistered;
    private bool exitRegistered;
    private bool allowExit;

    public Form1()
    {
        startInBackground = Environment.GetCommandLineArgs()
            .Any(arg => string.Equals(
                arg, "--background",
                StringComparison.OrdinalIgnoreCase));

        Text = "QuickSearch";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(900, 114);
        MinimumSize = new Size(560, 154);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10);
        KeyPreview = true;

        if (startInBackground)
        {
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
        }

        taglineLabel = QuickSearchUI.Build(
            this,
            searchBox,
            webView,
            backButton,
            forwardButton,
            refreshButton,
            searchButton,
            chromeButton,
            statusLabel);

        searchBox.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await SearchAsync();
            }
        };

        searchButton.Click += async (_, _) => await SearchAsync();

        backButton.Click += (_, _) =>
        {
            if (browserReady && webView.CoreWebView2.CanGoBack)
                webView.CoreWebView2.GoBack();
        };

        forwardButton.Click += (_, _) =>
        {
            if (browserReady && webView.CoreWebView2.CanGoForward)
                webView.CoreWebView2.GoForward();
        };

        refreshButton.Click += (_, _) =>
        {
            if (browserReady)
                webView.CoreWebView2.Reload();
        };

        chromeButton.Click += (_, _) => OpenInChrome();

        searchBox.TextChanged += (_, _) => UpdateNavigationButtons();

        Shown += async (_, _) =>
        {
            toggleRegistered = RegisterHotKey(
                Handle, ToggleHotkey,
                ModControl | ModAlt, VkSpace);

            exitRegistered = RegisterHotKey(
                Handle, ExitHotkey,
                ModControl | ModAlt, VkQ);

            if (!toggleRegistered || !exitRegistered)
            {
                MessageBox.Show(
                    "One or more QuickSearch hotkeys could not be registered. " +
                    "Another application may already be using them.",
                    "QuickSearch",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            if (startInBackground)
                Hide();

            await InitializeBrowserAsync();

            if (!startInBackground)
                searchBox.Focus();
        };

        FormClosing += HandleFormClosing;

        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (resultsVisible)
                    HideResults();
                else
                    Hide();

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        };
    }

    private void HandleFormClosing(
        object? sender, FormClosingEventArgs e)
    {
        if (!allowExit && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        UnregisterHotkeys();
    }

    private void UnregisterHotkeys()
    {
        if (toggleRegistered)
        {
            UnregisterHotKey(Handle, ToggleHotkey);
            toggleRegistered = false;
        }

        if (exitRegistered)
        {
            UnregisterHotKey(Handle, ExitHotkey);
            exitRegistered = false;
        }
    }

    private void ExitApplication()
    {
        allowExit = true;
        UnregisterHotkeys();
        Close();
    }

    private async Task InitializeBrowserAsync()
    {
        try
        {
            statusLabel.Text = "Preparing browser...";

            string profile = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "QuickSearch", "WebView2");

            Directory.CreateDirectory(profile);

            var environment =
                await CoreWebView2Environment.CreateAsync(
                    userDataFolder: profile);

            await webView.EnsureCoreWebView2Async(environment);

            webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;

            webView.CoreWebView2.NavigationStarting += (_, _) =>
            {
                statusLabel.Text = "Loading...";
            };

            webView.CoreWebView2.NavigationCompleted += (_, e) =>
            {
                statusLabel.Text = e.IsSuccess
                    ? "Ready"
                    : "Unable to load page";

                UpdateNavigationButtons();
            };

            browserReady = true;
            statusLabel.Text = "Ready";
            UpdateNavigationButtons();
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Browser initialization failed";

            MessageBox.Show(
                "Unable to initialize WebView2.\n\n" + ex.Message,
                "QuickSearch",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async Task SearchAsync()
    {
        string query = searchBox.Text.Trim();

        if (query.Length == 0)
        {
            searchBox.Focus();
            return;
        }

        if (!browserReady)
        {
            statusLabel.Text = "Browser is still starting...";
            return;
        }

        ShowResults();

        string url =
            "https://www.google.com/search?q=" +
            Uri.EscapeDataString(query);

        webView.CoreWebView2.Navigate(url);

        await Task.CompletedTask;
    }

    private void ShowResults()
    {
        resultsVisible = true;
        taglineLabel.Visible = false;
        webView.Visible = true;

        ShowInTaskbar = true;
        WindowState = FormWindowState.Normal;
        ClientSize = new Size(900, 680);

        Show();
        Activate();
        searchBox.Focus();
        searchBox.SelectAll();

        UpdateNavigationButtons();
    }

    private void HideResults()
    {
        resultsVisible = false;
        taglineLabel.Visible = true;
        webView.Visible = false;

        ClientSize = new Size(900, 114);

        searchBox.Focus();
        UpdateNavigationButtons();
    }

    private void UpdateNavigationButtons()
    {
        bool canNavigate = browserReady;

        backButton.Enabled =
            canNavigate && webView.CoreWebView2.CanGoBack;

        forwardButton.Enabled =
            canNavigate && webView.CoreWebView2.CanGoForward;

        refreshButton.Enabled = canNavigate;
    }

    private void OpenInChrome()
    {
        string url = browserReady
            ? webView.Source?.ToString() ?? "https://www.google.com"
            : "https://www.google.com";

        string[] paths =
        {
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFiles),
                "Google", "Chrome", "Application", "chrome.exe"),

            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFilesX86),
                "Google", "Chrome", "Application", "chrome.exe"),

            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Google", "Chrome", "Application", "chrome.exe")
        };

        foreach (string path in paths)
        {
            if (!File.Exists(path))
                continue;

            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                Arguments = "\"" + url + "\"",
                UseShellExecute = true
            });

            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotkey)
        {
            switch (m.WParam.ToInt32())
            {
                case ToggleHotkey:
                    if (Visible)
                    {
                        Hide();
                    }
                    else
                    {
                        ShowInTaskbar = true;
                        Show();
                        WindowState = FormWindowState.Normal;
                        Activate();
                        searchBox.Focus();
                    }
                    break;

                case ExitHotkey:
                    ExitApplication();
                    break;
            }
        }

        base.WndProc(ref m);
    }
}
