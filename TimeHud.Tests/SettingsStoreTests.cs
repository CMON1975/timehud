using TimeHud;

namespace TimeHud.Tests;

public class SettingsStoreTests : IDisposable
{
    private readonly string _dir;
    private readonly string _path;

    public SettingsStoreTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "TimeHudTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
        _path = Path.Combine(_dir, "settings.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }

    [Fact]
    public void Load_returns_defaults_when_file_is_missing()
    {
        var s = SettingsStore.Load(_path);
        Assert.Equal(Settings.Default.Opacity, s.Opacity, 3);
        Assert.Equal(Settings.Default.FontSize, s.FontSize);
        Assert.Equal(Settings.Default.FontKey, s.FontKey);
    }

    [Fact]
    public void Load_returns_defaults_when_file_is_corrupt()
    {
        File.WriteAllText(_path, "{ this is not valid json");
        var s = SettingsStore.Load(_path);
        Assert.Equal(Settings.Default.Opacity, s.Opacity, 3);
        Assert.Equal(Settings.Default.FontKey, s.FontKey);
    }

    [Fact]
    public void Save_then_Load_round_trips_all_fields()
    {
        var original = new Settings
        {
            X = 123.5,
            Y = 678.25,
            Opacity = 0.4,
            FontSize = 96,
            FontKey = "cascadia",
            Color = "#FFB000"
        };
        SettingsStore.Save(_path, original);

        var loaded = SettingsStore.Load(_path);
        Assert.Equal(original.X!.Value, loaded.X!.Value, 3);
        Assert.Equal(original.Y!.Value, loaded.Y!.Value, 3);
        Assert.Equal(original.Opacity, loaded.Opacity, 3);
        Assert.Equal(original.FontSize, loaded.FontSize);
        Assert.Equal(original.FontKey, loaded.FontKey);
        Assert.Equal(original.Color, loaded.Color);
    }

    [Fact]
    public void Default_color_is_phosphor_green()
    {
        Assert.Equal("#00FF5A", Settings.Default.Color);
    }

    [Fact]
    public void Default_font_key_is_cascadiamono()
    {
        Assert.Equal("cascadiamono", Settings.Default.FontKey);
    }

    [Fact]
    public void Save_creates_parent_directory_if_missing()
    {
        var nestedDir = Path.Combine(_dir, "nested", "deeper");
        var nestedPath = Path.Combine(nestedDir, "settings.json");
        Assert.False(Directory.Exists(nestedDir));

        SettingsStore.Save(nestedPath, new Settings { FontKey = "consolas" });

        Assert.True(File.Exists(nestedPath));
    }
}
