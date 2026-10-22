
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace QuickSearch;

internal static class QuickSearchUI
{
    private static readonly Color Background = Color.FromArgb(245, 247, 250);
    private static readonly Color Accent = Color.FromArgb(35, 105, 220);
    private static readonly Color Border = Color.FromArgb(220, 225, 233);
    private static readonly Color TextColor = Color.FromArgb(35, 43, 56);

    public static Label Build(
        Form form,
        TextBox searchBox,
        WebView2 webView,
        Button backButton,
        Button forwardButton,
        Button refreshButton,
        Button searchButton,
        Button chromeButton,
        Label statusLabel)
    {
        form.SuspendLayout();

        var toolbar = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 66,
            ColumnCount = 8,
            RowCount = 1,
            Padding = new Padding(12, 10, 12, 10),
            BackColor = Color.White
        };

        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 8));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126));
        toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        PictureBox logo = CreateLogo();
        logo.Dock = DockStyle.Fill;

        ConfigureNavigationButton(backButton, "←", "Back");
        ConfigureNavigationButton(forwardButton, "→", "Forward");
        ConfigureNavigationButton(refreshButton, "⟳", "Refresh");

        searchBox.Dock = DockStyle.Fill;
        searchBox.AutoSize = false;
        searchBox.Font = new Font("Segoe UI", 12);
        searchBox.BorderStyle = BorderStyle.FixedSingle;
        searchBox.Margin = new Padding(5, 2, 8, 2);
        searchBox.PlaceholderText = "Search Google...";
        searchBox.BackColor = Color.White;
        searchBox.ForeColor = TextColor;

        ConfigureActionButton(searchButton, "Search", true);
        ConfigureActionButton(chromeButton, "To Chrome", false);

        toolbar.Controls.Add(logo, 0, 0);
        toolbar.Controls.Add(backButton, 1, 0);
        toolbar.Controls.Add(forwardButton, 2, 0);
        toolbar.Controls.Add(refreshButton, 3, 0);
        toolbar.Controls.Add(searchBox, 4, 0);
        toolbar.Controls.Add(searchButton, 5, 0);
        toolbar.Controls.Add(new Panel(), 6, 0);
        toolbar.Controls.Add(chromeButton, 7, 0);

        var searchArea = new Panel
        {
            Dock = DockStyle.Top,
            Height = 90,
            BackColor = Color.White
        };

        var taglineLabel = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 24,
            Text = "A very fast shortcut for a problem that is small -D",
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(155, 162, 173),
            BackColor = Color.White
        };

        taglineLabel.VisibleChanged += (_, _) =>
            searchArea.Height = taglineLabel.Visible ? 90 : 66;

        searchArea.Controls.Add(taglineLabel);
        searchArea.Controls.Add(toolbar);

        webView.Dock = DockStyle.Fill;
        webView.Visible = false;
        webView.Margin = Padding.Empty;

        statusLabel.Dock = DockStyle.Bottom;
        statusLabel.Height = 24;
        statusLabel.Padding = new Padding(12, 3, 0, 0);
        statusLabel.Font = new Font("Segoe UI", 9);
        statusLabel.BackColor = Color.White;
        statusLabel.ForeColor = Color.FromArgb(105, 113, 126);
        statusLabel.Text = "Ready";

        form.BackColor = Background;
        form.Controls.Add(webView);
        form.Controls.Add(statusLabel);
        form.Controls.Add(searchArea);

        form.ResumeLayout(true);
        return taglineLabel;
    }

    private static PictureBox CreateLogo()
    {
        var logo = new PictureBox
        {
            Size = new Size(40, 40),
            SizeMode = PictureBoxSizeMode.Zoom,
            Margin = new Padding(2)
        };

        string path = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "QuickSearch.png");

        if (File.Exists(path))
        {
            using var image = Image.FromFile(path);
            logo.Image = new Bitmap(image);
        }

        return logo;
    }

    private static void ConfigureNavigationButton(
        Button button, string text, string tooltip)
    {
        button.Text = text;
        button.Dock = DockStyle.Fill;
        button.Margin = new Padding(2);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Border;
        button.FlatAppearance.BorderSize = 1;
        button.BackColor = Color.White;
        button.ForeColor = TextColor;
        button.Font = new Font("Segoe UI", 15);
        button.Cursor = Cursors.Hand;
        button.AccessibleName = tooltip;
    }

    private static void ConfigureActionButton(
        Button button, string text, bool primary)
    {
        button.Text = text;
        button.Dock = DockStyle.Fill;
        button.Margin = new Padding(2);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = primary ? 0 : 1;
        button.FlatAppearance.BorderColor = Border;
        button.BackColor = primary ? Accent : Color.White;
        button.ForeColor = primary ? Color.White : TextColor;
        button.Font = new Font("Segoe UI", 9.5f);
        button.Cursor = Cursors.Hand;
    }
}
