using System.Text.RegularExpressions;
using TimeHud;

namespace TimeHud.Tests;

public class ColorPaletteTests
{
    [Fact]
    public void All_lists_five_presets_in_HUD_order()
    {
        var keys = ColorPalette.All.Select(c => c.Key).ToArray();
        Assert.Equal(new[] { "green", "amber", "cyan", "white", "red" }, keys);
    }

    [Fact]
    public void Default_key_is_green()
    {
        Assert.Equal("green", ColorPalette.DefaultKey);
    }

    [Fact]
    public void Default_hex_matches_Settings_Default_Color()
    {
        var defaultPreset = ColorPalette.Lookup(ColorPalette.DefaultKey);
        Assert.Equal(defaultPreset.Hex, Settings.Default.Color);
    }

    [Fact]
    public void Each_preset_has_a_nonempty_display_name()
    {
        foreach (var c in ColorPalette.All)
            Assert.False(string.IsNullOrWhiteSpace(c.DisplayName));
    }

    [Fact]
    public void Each_preset_has_a_valid_hex()
    {
        var hexPattern = new Regex("^#[0-9A-Fa-f]{6}$");
        foreach (var c in ColorPalette.All)
            Assert.Matches(hexPattern, c.Hex);
    }

    [Fact]
    public void Lookup_unknown_key_falls_back_to_default()
    {
        Assert.Equal(ColorPalette.DefaultKey, ColorPalette.Lookup("nonsense").Key);
    }

    [Fact]
    public void Green_preset_is_phosphor_green()
    {
        Assert.Equal("#00FF5A", ColorPalette.Lookup("green").Hex);
    }
}
