using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
    private readonly DispatcherTimer _countdownTimer = new() { Interval = TimeSpan.FromMilliseconds(200) };
    private readonly AutostartManager _autostart = new(new RegistryStore());
    private readonly ISoundPlayer _sound = new TonePlayer();

    // Lucide "play" and "rotate-ccw" (24x24 viewBox, stroke 2, round caps/joins).
    private static readonly Geometry PlayIcon  = Geometry.Parse("M6 3 L20 12 L6 21 Z");
    private static readonly Geometry ResetIcon = Geometry.Parse("M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8 M3 3v5h5");

    private OpacityModel _opacity = new(0.75);
    private OpacityModel _textOpacity = new(1.00);
    private SizeModel _size = new(48);
    private string _fontKey = FontRegistry.DefaultKey;
    private string _colorHex = ColorPalette.Lookup(ColorPalette.DefaultKey).Hex;
    private CountdownModel _countdown = new(30);
    private bool _showTimer = true;
    private bool _flashing;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var s = SettingsStore.Load(SettingsPath);
        SettingsStore.Archive(SettingsPath);
        _opacity = new OpacityModel(s.Opacity);
        _textOpacity = new OpacityModel(s.TextOpacity);
        _size = new SizeModel(s.FontSize);
        _fontKey = s.FontKey;
        _colorHex = s.Color;
        _countdown = new CountdownModel(s.TimerMinutes);
        _showTimer = s.ShowTimer;

        if (s.X is double x && s.Y is double y)
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = x;
            Top = y;
        }

        Backdrop.Opacity = _opacity.Value;
        ApplyTextAlpha();
        ApplySize(_size.Value);
        ApplyFont(_fontKey);
        ApplyColor(_colorHex);
        ClockText.Text = ClockFormatter.Format(DateTime.Now);
        ApplyShowTimer();
        RefreshTimerUi();

        SyncFontMenuChecks();
        SyncColorMenuChecks();
        SyncSizeMenuChecks();
        SyncTextAlphaMenuChecks();
        SyncTimerMenuChecks();
        AutostartMenu.IsChecked = _autostart.IsEnabled();

        _clockTimer.Tick += (_, _) => ClockText.Text = ClockFormatter.Format(DateTime.Now);
        _clockTimer.Start();

        _countdownTimer.Tick += OnCountdownTick;

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
            ApplySize(_size.Value);
            SyncSizeMenuChecks();
        }
        else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            if (e.Delta > 0) _textOpacity.StepUp(); else _textOpacity.StepDown();
            ApplyTextAlpha();
            SyncTextAlphaMenuChecks();
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

    private void OnSizePresetClick(object sender, RoutedEventArgs e)
    {
        _size = new SizeModel(int.Parse((string)((MenuItem)sender).Tag, CultureInfo.InvariantCulture));
        ApplySize(_size.Value);
        SyncSizeMenuChecks();
        SaveCurrent();
    }

    private void OnTextAlphaPresetClick(object sender, RoutedEventArgs e)
    {
        _textOpacity = new OpacityModel(double.Parse((string)((MenuItem)sender).Tag, CultureInfo.InvariantCulture));
        ApplyTextAlpha();
        SyncTextAlphaMenuChecks();
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

    // Font, color and size are set on Row as TextElement attached properties and inherited by
    // the clock and timer TextBlocks; the divider and icon are shapes, so they get the brush directly.
    private void ApplyFont(string key) =>
        TextElement.SetFontFamily(Row, FontFamilyFactory.For(FontRegistry.Lookup(key)));

    private void ApplyColor(string hex)
    {
        var brush = HexToBrush(hex);
        TextElement.SetForeground(Row, brush);
        Divider.Fill = brush;
        TimerIcon.Stroke = brush;
    }

    private void ApplySize(int pt)
    {
        TextElement.SetFontSize(Row, pt);
        TimerIconBox.Width = TimerIconBox.Height = pt * 0.8;
    }

    private static SolidColorBrush HexToBrush(string hex) =>
        new((SwmColor)SwmColorConverter.ConvertFromString(hex));

    // ---- Stand-up timer ----

    private void OnTimerButtonClick(object sender, RoutedEventArgs e)
    {
        _countdown.Start(DateTime.UtcNow);
        StopFlash();
        RefreshTimerUi();
        _countdownTimer.Start();
    }

    private void OnCountdownTick(object? sender, EventArgs e)
    {
        var fx = _countdown.Tick(DateTime.UtcNow);
        if (fx.HasFlag(TickEffects.Beep)) _sound.Beep();
        TimerText.Text = CountdownFormatter.Format(_countdown.RemainingSeconds);
        if (fx.HasFlag(TickEffects.Finished))
        {
            _countdownTimer.Stop();
            _sound.BeepLong();
            StartFlash();
        }
    }

    private void OnTimerPresetClick(object sender, RoutedEventArgs e)
    {
        var minutes = int.Parse((string)((MenuItem)sender).Tag, CultureInfo.InvariantCulture);
        _countdown.SetDuration(minutes, DateTime.UtcNow);
        if (_countdown.State == CountdownState.Running)
        {
            StopFlash();
            _countdownTimer.Start();
        }
        RefreshTimerUi();
        SyncTimerMenuChecks();
        SaveCurrent();
    }

    private void OnShowTimerClick(object sender, RoutedEventArgs e)
    {
        _showTimer = ShowTimerMenu.IsChecked;
        if (!_showTimer)
        {
            _countdownTimer.Stop();
            StopFlash();
            _countdown.Cancel();
            RefreshTimerUi();
        }
        ApplyShowTimer();
        SaveCurrent();
    }

    private void ApplyShowTimer()
    {
        TimerGroup.Visibility = _showTimer ? Visibility.Visible : Visibility.Collapsed;
        ShowTimerMenu.IsChecked = _showTimer;
    }

    private void RefreshTimerUi()
    {
        TimerText.Text = CountdownFormatter.Format(_countdown.RemainingSeconds);
        TimerIcon.Data = _countdown.State == CountdownState.Idle ? PlayIcon : ResetIcon;
    }

    // Text alpha is applied per element rather than on Row, so the finished countdown can be pushed
    // to full opacity on its own: a child can never be more opaque than its parent.
    private void ApplyTextAlpha()
    {
        var a = _textOpacity.Value;
        ClockText.Opacity = a;
        Divider.Opacity = 0.5 * a;
        TimerButton.Opacity = a;
        TimerText.Opacity = _flashing ? 1.0 : a;
    }

    // Flash: red foreground + blink animation on TimerText.Opacity from a base of 1.0 (text alpha
    // is dropped for the countdown while it's at 00:00). Foreground is a local override that
    // ClearValue returns to the inherited color.
    private void StartFlash()
    {
        _flashing = true;
        ApplyTextAlpha();
        TimerText.Foreground = HexToBrush(ColorPalette.Lookup("red").Hex);
        var blink = new DoubleAnimation(1.0, 0.15, TimeSpan.FromMilliseconds(400))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
        };
        TimerText.BeginAnimation(OpacityProperty, blink);
    }

    private void StopFlash()
    {
        _flashing = false;
        TimerText.BeginAnimation(OpacityProperty, null);
        TimerText.ClearValue(ForegroundProperty);
        ApplyTextAlpha();
    }

    private void SyncTimerMenuChecks()
    {
        foreach (var item in TimerMenu.Items.OfType<MenuItem>())
            if (item.Tag is string tag)
                item.IsChecked = int.Parse(tag, CultureInfo.InvariantCulture) == _countdown.DurationMinutes;
    }

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

    private void SyncSizeMenuChecks()
    {
        foreach (var item in SizeMenu.Items.OfType<MenuItem>())
            if (item.Tag is string tag)
                item.IsChecked = int.Parse(tag, CultureInfo.InvariantCulture) == _size.Value;
    }

    private void SyncTextAlphaMenuChecks()
    {
        foreach (var item in TextAlphaMenu.Items.OfType<MenuItem>())
            if (item.Tag is string tag)
                item.IsChecked = Math.Abs(double.Parse(tag, CultureInfo.InvariantCulture) - _textOpacity.Value) < 0.005;
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
        TextOpacity = _textOpacity.Value,
        FontSize = _size.Value,
        FontKey = _fontKey,
        Color = _colorHex,
        TimerMinutes = _countdown.DurationMinutes,
        ShowTimer = _showTimer,
    });
}
