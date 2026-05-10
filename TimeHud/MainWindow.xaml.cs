using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SwfColorDialog = System.Windows.Forms.ColorDialog;
using SwfDialogResult = System.Windows.Forms.DialogResult;
using SdColor = System.Drawing.Color;
using SwmColor = System.Windows.Media.Color;
using SwmColorConverter = System.Windows.Media.ColorConverter;

namespace TimeHud;

public partial class MainWindow : Window
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TimeHud", "settings.json");

    private readonly DispatcherTimer _clockTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly DispatcherTimer _topmostTimer = new() { Interval = TimeSpan.FromSeconds(2) };
    private readonly AutostartManager _autostart = new(new RegistryStore());

    private OpacityModel _opacity = new(0.75);
    private SizeModel _size = new(48);
    private string _fontKey = FontRegistry.DefaultKey;
    private string _colorHex = ColorPalette.Lookup(ColorPalette.DefaultKey).Hex;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var s = SettingsStore.Load(SettingsPath);
        _opacity = new OpacityModel(s.Opacity);
        _size = new SizeModel(s.FontSize);
        _fontKey = s.FontKey;
        _colorHex = s.Color;

        if (s.X is double x && s.Y is double y)
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = x;
            Top = y;
        }

        Backdrop.Opacity = _opacity.Value;
        ClockText.FontSize = _size.Value;
        ApplyFont(_fontKey);
        ApplyColor(_colorHex);
        ClockText.Text = ClockFormatter.Format(DateTime.Now);

        SyncFontMenuChecks();
        SyncColorMenuChecks();
        AutostartMenu.IsChecked = _autostart.IsEnabled();

        _clockTimer.Tick += (_, _) => ClockText.Text = ClockFormatter.Format(DateTime.Now);
        _clockTimer.Start();

        _topmostTimer.Tick += (_, _) => TopmostKeeper.Reassert(this);
        _topmostTimer.Start();
    }

    private void OnRootMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) DragMove();
    }

    private void OnRootMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            if (e.Delta > 0) _size.StepUp(); else _size.StepDown();
            ClockText.FontSize = _size.Value;
        }
        else
        {
            if (e.Delta > 0) _opacity.StepUp(); else _opacity.StepDown();
            Backdrop.Opacity = _opacity.Value;
        }
        SaveCurrent();
        e.Handled = true;
    }

    private void OnFontPresetClick(object sender, RoutedEventArgs e)
    {
        var key = (string)((MenuItem)sender).Tag;
        _fontKey = key;
        ApplyFont(key);
        SyncFontMenuChecks();
        SaveCurrent();
    }

    private void OnColorPresetClick(object sender, RoutedEventArgs e)
    {
        var key = (string)((MenuItem)sender).Tag;
        SetColor(ColorPalette.Lookup(key).Hex);
    }

    private void OnColorCustomClick(object sender, RoutedEventArgs e)
    {
        using var dlg = new SwfColorDialog
        {
            Color = HexToDrawingColor(_colorHex),
            FullOpen = true,
            AnyColor = true,
        };
        if (dlg.ShowDialog() == SwfDialogResult.OK)
            SetColor(DrawingColorToHex(dlg.Color));
    }

    private void SetColor(string hex)
    {
        _colorHex = hex;
        ApplyColor(hex);
        SyncColorMenuChecks();
        SaveCurrent();
    }

    private void ApplyFont(string key) =>
        ClockText.FontFamily = FontFamilyFactory.For(FontRegistry.Lookup(key));

    private void ApplyColor(string hex) =>
        ClockText.Foreground = new SolidColorBrush((SwmColor)SwmColorConverter.ConvertFromString(hex));

    private void SyncFontMenuChecks()
    {
        FontCascadiaMono.IsChecked = _fontKey == "cascadiamono";
        FontCascadia.IsChecked     = _fontKey == "cascadia";
        FontConsolas.IsChecked     = _fontKey == "consolas";
        FontDseg7.IsChecked        = _fontKey == "dseg7";
        FontD7Mono.IsChecked       = _fontKey == "d7mono";
    }

    private void SyncColorMenuChecks()
    {
        ColorGreen.IsChecked = HexEquals(_colorHex, ColorPalette.Lookup("green").Hex);
        ColorAmber.IsChecked = HexEquals(_colorHex, ColorPalette.Lookup("amber").Hex);
        ColorCyan.IsChecked  = HexEquals(_colorHex, ColorPalette.Lookup("cyan").Hex);
        ColorWhite.IsChecked = HexEquals(_colorHex, ColorPalette.Lookup("white").Hex);
        ColorRed.IsChecked   = HexEquals(_colorHex, ColorPalette.Lookup("red").Hex);
    }

    private static bool HexEquals(string a, string b) =>
        string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    private static SdColor HexToDrawingColor(string hex)
    {
        var c = (SwmColor)SwmColorConverter.ConvertFromString(hex);
        return SdColor.FromArgb(c.A, c.R, c.G, c.B);
    }

    private static string DrawingColorToHex(SdColor c) =>
        $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    private void OnAutostartClick(object sender, RoutedEventArgs e)
    {
        if (AutostartMenu.IsChecked)
            _autostart.Enable(Environment.ProcessPath ?? throw new InvalidOperationException("ProcessPath unavailable"));
        else
            _autostart.Disable();
    }

    private void OnResetPositionClick(object sender, RoutedEventArgs e)
    {
        var area = SystemParameters.WorkArea;
        Left = (area.Width - ActualWidth) / 2;
        Top = (area.Height - ActualHeight) / 2;
        SaveCurrent();
    }

    private void OnExitClick(object sender, RoutedEventArgs e) => Close();

    private void OnClosing(object? sender, CancelEventArgs e) => SaveCurrent();

    private void SaveCurrent() => SettingsStore.Save(SettingsPath, new Settings
    {
        X = Left,
        Y = Top,
        Opacity = _opacity.Value,
        FontSize = _size.Value,
        FontKey = _fontKey,
        Color = _colorHex,
    });
}
