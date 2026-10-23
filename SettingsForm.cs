#nullable enable

using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace QuickSearch;

internal static class QuickSearchSettings
{
    private sealed class Data
    {
        public bool DarkMode { get; set; }
        public string SearchEngine { get; set; } = "Google";
        public bool AlwaysOnTop { get; set; }
        public bool CenterOnShow { get; set; } = true;
        public bool HideAfterSearch { get; set; }
        public bool SelectSearchText { get; set; } = true;
        public int WindowWidth { get; set; } = 900;
        public int WindowHeight { get; set; } = 680;
        public string AccentColor { get; set; } = "#2369DC";
        public string ToggleModifiers { get; set; } = "Control, Alt";
        public string ToggleKey { get; set; } = "Space";
        public string ExitModifiers { get; set; } = "Control, Alt";
        public string ExitKey { get; set; } = "Q";
    }

    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
        "QuickSearch", "settings.json");

    private static Data data = LoadData();

    public static bool DarkMode
    {
        get => data.DarkMode;
        set => data.DarkMode = value;
    }

    public static string SearchEngine
    {
        get => data.SearchEngine;
        set => data.SearchEngine = value;
    }

    public static bool AlwaysOnTop
    {
        get => data.AlwaysOnTop;
        set => data.AlwaysOnTop = value;
    }

    public static bool CenterOnShow
    {
        get => data.CenterOnShow;
        set => data.CenterOnShow = value;
    }

    public static bool HideAfterSearch
    {
        get => data.HideAfterSearch;
        set => data.HideAfterSearch = value;
    }

    public static bool SelectSearchText
    {
        get => data.SelectSearchText;
        set => data.SelectSearchText = value;
    }

    public static int WindowWidth
    {
        get => data.WindowWidth;
        set => data.WindowWidth = Math.Clamp(value, 560, 1920);
    }

    public static int WindowHeight
    {
        get => data.WindowHeight;
        set => data.WindowHeight = Math.Clamp(value, 400, 1200);
    }

    public static string AccentColor
    {
        get => data.AccentColor;
        set => data.AccentColor = value;
    }

    public static string ToggleModifiers
    {
        get => data.ToggleModifiers;
        set => data.ToggleModifiers = value;
    }

    public static string ToggleKey
    {
        get => data.ToggleKey;
        set => data.ToggleKey = value;
    }

    public static string ExitModifiers
    {
        get => data.ExitModifiers;
        set => data.ExitModifiers = value;
    }

    public static string ExitKey
    {
        get => data.ExitKey;
        set => data.ExitKey = value;
    }

    public static Color GetAccent()
    {
        try
        {
            return ColorTranslator.FromHtml(AccentColor);
        }
        catch
        {
            return Color.FromArgb(35, 105, 220);
        }
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

            File.WriteAllText(
                FilePath,
                JsonSerializer.Serialize(
                    data,
                    new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Could not save QuickSearch settings.\n\n" + ex.Message,
                "QuickSearch",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private static Data LoadData()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<Data>(
                    File.ReadAllText(FilePath)) ?? new Data();
        }
        catch
        {
            // Use defaults if the settings file is invalid.
        }

        return new Data();
    }
}

internal sealed class SettingsForm : Form
{
    public event EventHandler? SettingsChanged;

    private readonly CheckBox dark = new();
    private readonly CheckBox top = new();
    private readonly CheckBox center = new();
    private readonly CheckBox hideAfter = new();
    private readonly CheckBox selectText = new();

    private readonly ComboBox engine = new();
    private readonly ComboBox toggleModifiers = new();
    private readonly ComboBox toggleKey = new();
    private readonly ComboBox exitModifiers = new();
    private readonly ComboBox exitKey = new();

    private readonly NumericUpDown width = new();
    private readonly NumericUpDown height = new();
    private readonly Button accentButton = new();
    private readonly Button saveButton = new();

    public SettingsForm()
    {
        Text = "QuickSearch Settings";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(560, 700);
        MinimumSize = new Size(560, 700);
        MaximumSize = new Size(560, 700);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoScroll = false;
        Font = new Font("Segoe UI", 9.5f);

        var content = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(20)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 0,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));

        void Section(string text)
        {
            int row = layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.Controls.Add(new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 8, 0, 2)
            }, 0, row);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, row)!, 2);
        }

        void AddCheck(CheckBox check, string text)
        {
            check.Text = text;
            check.AutoSize = true;
            check.Anchor = AnchorStyles.Left;
            int row = layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.Controls.Add(check, 0, row);
            layout.SetColumnSpan(check, 2);
        }

        void AddPair(string label, Control control)
        {
            int row = layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));

            layout.Controls.Add(new Label
            {
                Text = label,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 8, 0, 0)
            }, 0, row);

            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(4, 3, 0, 3);
            layout.Controls.Add(control, 1, row);
        }

        Section("Appearance");
        AddCheck(dark, "Dark mode");
        AddPair("Accent color", accentButton);

        accentButton.Text = "Choose color...";
        accentButton.Click += (_, _) =>
        {
            using var dialog = new ColorDialog
            {
                Color = QuickSearchSettings.GetAccent(),
                FullOpen = true
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                QuickSearchSettings.AccentColor =
                    ColorTranslator.ToHtml(dialog.Color);
                accentButton.BackColor = dialog.Color;
                accentButton.ForeColor =
                    dialog.Color.GetBrightness() < 0.5
                        ? Color.White : Color.Black;
            }
        };

        Section("Search");
        engine.DropDownStyle = ComboBoxStyle.DropDownList;
        engine.Items.AddRange(new object[]
        {
            "Google", "Bing", "DuckDuckGo"
        });
        AddPair("Search engine", engine);
        AddCheck(selectText, "Select the search text when the window opens");

        Section("Window behavior");
        AddCheck(top, "Keep QuickSearch on top of other windows");
        AddCheck(center, "Center the window when it opens");
        AddCheck(hideAfter, "Hide QuickSearch after submitting a search");
        AddPair("Window width", width);
        AddPair("Window height", height);

        width.Minimum = 560;
        width.Maximum = 1920;
        width.Increment = 50;

        height.Minimum = 400;
        height.Maximum = 1200;
        height.Increment = 50;

        Section("Global hotkeys");

        ConfigureCombo(toggleModifiers,
            "None", "Control", "Alt", "Control, Alt",
            "Shift", "Control, Shift", "Alt, Shift",
            "Control, Alt, Shift");

        ConfigureCombo(exitModifiers,
            "None", "Control", "Alt", "Control, Alt",
            "Shift", "Control, Shift", "Alt, Shift",
            "Control, Alt, Shift");

        ConfigureCombo(toggleKey,
            "Space", "Q", "W", "E", "R", "F", "G",
            "J", "K", "L", "P", "F1", "F2", "F3",
            "F4", "F5", "F6", "F7", "F8", "F9", "F10",
            "F11", "F12");

        ConfigureCombo(exitKey,
            "Space", "Q", "W", "E", "R", "F", "G",
            "J", "K", "L", "P", "F1", "F2", "F3",
            "F4", "F5", "F6", "F7", "F8", "F9", "F10",
            "F11", "F12");

        AddPair("Show / hide modifiers", toggleModifiers);
        AddPair("Show / hide key", toggleKey);
        AddPair("Exit modifiers", exitModifiers);
        AddPair("Exit key", exitKey);

        Section("About");

        var aboutButton = new Button
        {
            Text = "About QuickSearch",
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 3, 4, 3)
        };
        aboutButton.Click += (_, _) => ShowInformationDialog(
            "About QuickSearch",
            $"QuickSearch\nVersion v{Application.ProductVersion}\n\n" +
            "A fast shortcut for searching the web.");

        var licenseButton = new Button
        {
            Text = "License",
            Dock = DockStyle.Fill,
            Margin = new Padding(4, 3, 0, 3)
        };
        licenseButton.Click += (_, _) => ShowInformationDialog(
            "QuickSearch License",
            ReadLicenseText());

        int informationRow = layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.Controls.Add(aboutButton, 0, informationRow);
        layout.Controls.Add(licenseButton, 1, informationRow);

        saveButton.Text = "Save settings";
        saveButton.Height = 38;
        saveButton.Dock = DockStyle.Bottom;
        saveButton.BackColor = QuickSearchSettings.GetAccent();
        saveButton.ForeColor = Color.Black;
        saveButton.FlatStyle = FlatStyle.Flat;

        saveButton.Click += (_, _) =>
        {
            if (toggleKey.SelectedItem?.ToString() ==
                    exitKey.SelectedItem?.ToString() &&
                toggleModifiers.SelectedItem?.ToString() ==
                    exitModifiers.SelectedItem?.ToString())
            {
                MessageBox.Show(
                    "The show/hide and exit shortcuts must be different.",
                    "QuickSearch",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            QuickSearchSettings.DarkMode = dark.Checked;
            QuickSearchSettings.SearchEngine =
                engine.SelectedItem?.ToString() ?? "Google";
            QuickSearchSettings.AlwaysOnTop = top.Checked;
            QuickSearchSettings.CenterOnShow = center.Checked;
            QuickSearchSettings.HideAfterSearch = hideAfter.Checked;
            QuickSearchSettings.SelectSearchText = selectText.Checked;
            QuickSearchSettings.WindowWidth = (int)width.Value;
            QuickSearchSettings.WindowHeight = (int)height.Value;
            QuickSearchSettings.ToggleModifiers =
                toggleModifiers.SelectedItem?.ToString() ?? "Control, Alt";
            QuickSearchSettings.ToggleKey =
                toggleKey.SelectedItem?.ToString() ?? "Space";
            QuickSearchSettings.ExitModifiers =
                exitModifiers.SelectedItem?.ToString() ?? "Control, Alt";
            QuickSearchSettings.ExitKey =
                exitKey.SelectedItem?.ToString() ?? "Q";

            QuickSearchSettings.Save();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
            Close();
        };

        dark.Checked = QuickSearchSettings.DarkMode;
        top.Checked = QuickSearchSettings.AlwaysOnTop;
        center.Checked = QuickSearchSettings.CenterOnShow;
        hideAfter.Checked = QuickSearchSettings.HideAfterSearch;
        selectText.Checked = QuickSearchSettings.SelectSearchText;

        engine.SelectedItem = QuickSearchSettings.SearchEngine;
        if (engine.SelectedIndex < 0) engine.SelectedIndex = 0;

        width.Value = QuickSearchSettings.WindowWidth;
        height.Value = QuickSearchSettings.WindowHeight;

        SelectValue(toggleModifiers, QuickSearchSettings.ToggleModifiers);
        SelectValue(toggleKey, QuickSearchSettings.ToggleKey);
        SelectValue(exitModifiers, QuickSearchSettings.ExitModifiers);
        SelectValue(exitKey, QuickSearchSettings.ExitKey);

        accentButton.BackColor = QuickSearchSettings.GetAccent();

        content.Controls.Add(layout);
        Controls.Add(content);
        Controls.Add(saveButton);

        ApplyTheme();
    }

    private static void ConfigureCombo(
        ComboBox combo, params string[] values)
    {
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.Items.AddRange(values);
    }

    private static void SelectValue(ComboBox combo, string value)
    {
        combo.SelectedItem = value;
        if (combo.SelectedIndex < 0)
            combo.SelectedIndex = 0;
    }

    private void ApplyTheme()
    {
        bool isDark = QuickSearchSettings.DarkMode;
        Color background = isDark
            ? Color.FromArgb(30, 32, 36) : Color.White;
        Color foreground = isDark
            ? Color.White : Color.FromArgb(35, 43, 56);

        BackColor = background;
        ForeColor = foreground;

        foreach (Control control in Controls)
            ApplyThemeRecursive(control, background, foreground);

        accentButton.BackColor = QuickSearchSettings.GetAccent();
        accentButton.ForeColor =
            QuickSearchSettings.GetAccent().GetBrightness() < 0.5
                ? Color.White : Color.Black;
        saveButton.BackColor = QuickSearchSettings.GetAccent();
        saveButton.ForeColor = Color.Black;
    }

    private static void ApplyThemeRecursive(
        Control control, Color background, Color foreground)
    {
        control.BackColor = background;
        control.ForeColor = foreground;

        foreach (Control child in control.Controls)
            ApplyThemeRecursive(child, background, foreground);
    }

    private void ShowInformationDialog(string title, string text)
    {
        bool isDark = QuickSearchSettings.DarkMode;
        Color background = isDark
            ? Color.FromArgb(30, 32, 36) : Color.White;
        Color foreground = isDark
            ? Color.White : Color.FromArgb(35, 43, 56);

        using var dialog = new Form
        {
            Text = title,
            StartPosition = FormStartPosition.CenterParent,
            ClientSize = title == "QuickSearch License"
                ? new Size(640, 520)
                : new Size(420, 190),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            ShowInTaskbar = false,
            BackColor = background,
            ForeColor = foreground,
            Font = Font
        };

        var message = new TextBox
        {
            Text = text,
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            WordWrap = true,
            BorderStyle = BorderStyle.None,
            Padding = new Padding(18),
            BackColor = background,
            ForeColor = foreground,
            Font = Font
        };

        var closeButton = new Button
        {
            Text = "Close",
            Dock = DockStyle.Bottom,
            Height = 38,
            BackColor = QuickSearchSettings.GetAccent(),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        closeButton.Click += (_, _) => dialog.Close();

        dialog.Controls.Add(message);
        dialog.Controls.Add(closeButton);
        dialog.AcceptButton = closeButton;
        dialog.ShowDialog(this);
    }

    private static string ReadLicenseText()
    {
        using Stream? stream = typeof(SettingsForm).Assembly
            .GetManifestResourceStream("QuickSearch.LICENSE");

        if (stream == null)
            throw new InvalidOperationException(
                "The embedded LICENSE resource could not be found.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}