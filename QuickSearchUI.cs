#nullable enable

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace QuickSearch;

internal static class QuickSearchUI
{
    public static Label Build(
        Form form,
        TextBox searchBox,
        WebView2 webView,
        Button backButton,
        Button forwardButton,
        Button refreshButton,
        Button searchButton,
        Button chromeButton,
        Button settingsButton,
        Label statusLabel)
    {
        var toolbar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 9,
            RowCount = 1,
            Padding = new Padding(8),
            Margin = Padding.Empty
        };

        toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 8));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));

        PictureBox logo = new()
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            Margin = new Padding(2)
        };

        string logoPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "QuickSearch.png");

        if (File.Exists(logoPath))
        {
            using var image = Image.FromFile(logoPath);
            logo.Image = new Bitmap(image);
        }

        ConfigureButton(backButton, "←", "Back");
        ConfigureButton(forwardButton, "→", "Forward");
        ConfigureButton(refreshButton, "⟳", "Refresh");
        ConfigureButton(searchButton, "Search", "Search");
        ConfigureButton(chromeButton, "To Chrome", "Open in Chrome");
        ConfigureButton(settingsButton, "⚙", "Settings");

        backButton.Font = new Font("Segoe UI Symbol", 15, FontStyle.Bold);
        forwardButton.Font = new Font("Segoe UI Symbol", 15, FontStyle.Bold);
        settingsButton.Font = new Font("Segoe UI Symbol", 12);
        backButton.Paint += DrawNavigationGlyph;
        forwardButton.Paint += DrawNavigationGlyph;

        searchBox.Dock = DockStyle.Fill;
        searchBox.Font = new Font("Segoe UI", 12);
        searchBox.Margin = new Padding(5, 2, 6, 2);
        searchBox.BorderStyle = BorderStyle.FixedSingle;
        searchBox.PlaceholderText = "Search...";

        toolbar.Controls.Add(logo, 0, 0);
        toolbar.Controls.Add(backButton, 1, 0);
        toolbar.Controls.Add(forwardButton, 2, 0);
        toolbar.Controls.Add(refreshButton, 3, 0);
        toolbar.Controls.Add(searchBox, 4, 0);
        toolbar.Controls.Add(searchButton, 5, 0);
        toolbar.Controls.Add(new Panel(), 6, 0);
        toolbar.Controls.Add(chromeButton, 7, 0);
        toolbar.Controls.Add(settingsButton, 8, 0);

        var tagline = new Label
        {
            Text = "Very fast shortcut for a problem that is small.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 9)
        };

        webView.Dock = DockStyle.Fill;
        webView.Visible = false;
        webView.Margin = Padding.Empty;

        statusLabel.Dock = DockStyle.Fill;
        statusLabel.Padding = new Padding(10, 3, 0, 0);
        statusLabel.Text = "Ready";

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(tagline, 0, 1);
        layout.Controls.Add(webView, 0, 2);
        layout.Controls.Add(statusLabel, 0, 3);

        form.Controls.Add(layout);

        ApplyTheme(
            form, searchBox, backButton, forwardButton,
            refreshButton, searchButton, chromeButton,
            settingsButton, statusLabel);

        return tagline;
    }

    public static void ApplyTheme(
        Form form,
        TextBox searchBox,
        Button back,
        Button forward,
        Button refresh,
        Button search,
        Button chrome,
        Button settings,
        Label status)
    {
        bool dark = QuickSearchSettings.DarkMode;
        Color bg = dark
            ? Color.FromArgb(30, 32, 36)
            : Color.White;

        Color fg = dark
            ? Color.White
            : Color.FromArgb(35, 43, 56);

        Color border = dark
            ? Color.FromArgb(75, 79, 86)
            : Color.FromArgb(220, 225, 233);

        Color accent = QuickSearchSettings.GetAccent();

        form.BackColor = bg;
        form.ForeColor = fg;

        searchBox.BackColor = bg;
        searchBox.ForeColor = fg;

        status.BackColor = bg;
        status.ForeColor = fg;

        foreach (Button button in new[]
        {
            back, forward, refresh, search, chrome, settings
        })
        {
            bool isPrimary = button == search;

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = border;
            button.FlatAppearance.BorderSize = isPrimary ? 0 : 1;
            button.BackColor = isPrimary ? accent : bg;
            button.ForeColor = isPrimary && dark ? Color.White : fg;
        }

        foreach (Control control in form.Controls)
            ApplyThemeToChildren(control, bg, fg, accent, border, search);
    }

    private static void ApplyThemeToChildren(
        Control control,
        Color bg,
        Color fg,
        Color accent,
        Color border,
        Button primaryButton)
    {
        control.BackColor = bg;

        if (control is Label)
            control.ForeColor = fg;

        foreach (Control child in control.Controls)
        {
            if (child is Button button)
            {
                bool primary = button == primaryButton;
                button.BackColor = primary ? accent : bg;
                button.ForeColor = primary && QuickSearchSettings.DarkMode
                    ? Color.White
                    : fg;
                button.FlatAppearance.BorderColor = border;
            }
            else
            {
                child.BackColor = bg;
                child.ForeColor = fg;
            }

            ApplyThemeToChildren(
                child, bg, fg, accent, border, primaryButton);
        }
    }

    private static void ConfigureButton(
        Button button, string text, string accessibleName)
    {
        button.Text = text;
        button.Dock = DockStyle.Fill;
        button.Margin = new Padding(2);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.Font = new Font("Segoe UI", 9.5f);
        button.Cursor = Cursors.Hand;
        button.AccessibleName = accessibleName;
    }

    private static void DrawNavigationGlyph(
        object? sender, PaintEventArgs e)
    {
        if (sender is not Button button)
            return;

        Color foreground = QuickSearchSettings.DarkMode
            ? Color.White
            : Color.FromArgb(35, 43, 56);

        TextRenderer.DrawText(
            e.Graphics,
            button.Text,
            button.Font,
            button.ClientRectangle,
            foreground,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.SingleLine);
    }
}