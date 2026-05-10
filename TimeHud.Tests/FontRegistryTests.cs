using TimeHud;

namespace TimeHud.Tests;

public class FontRegistryTests
{
    [Fact]
    public void All_lists_modern_monos_first_then_digital_styles()
    {
        var keys = FontRegistry.All.Select(f => f.Key).ToArray();
        Assert.Equal(new[] { "cascadiamono", "cascadia", "consolas", "dseg7", "d7mono" }, keys);
    }

    [Fact]
    public void Default_key_is_cascadiamono()
    {
        Assert.Equal("cascadiamono", FontRegistry.DefaultKey);
    }

    [Fact]
    public void Lookup_returns_dseg7_bundled()
    {
        var f = FontRegistry.Lookup("dseg7");
        Assert.Equal("dseg7", f.Key);
        Assert.True(f.IsBundled);
        Assert.Contains("DSEG7", f.Source);
    }

    [Fact]
    public void Lookup_returns_d7mono_bundled()
    {
        var f = FontRegistry.Lookup("d7mono");
        Assert.Equal("d7mono", f.Key);
        Assert.True(f.IsBundled);
        Assert.Contains("Digital-7 Mono", f.Source);
    }

    [Fact]
    public void Lookup_returns_cascadia_system_font()
    {
        var f = FontRegistry.Lookup("cascadia");
        Assert.False(f.IsBundled);
        Assert.Equal("Cascadia Code", f.Source);
    }

    [Fact]
    public void Lookup_returns_cascadiamono_system_font()
    {
        var f = FontRegistry.Lookup("cascadiamono");
        Assert.False(f.IsBundled);
        Assert.Equal("Cascadia Mono", f.Source);
    }

    [Fact]
    public void Lookup_returns_consolas_system_font()
    {
        var f = FontRegistry.Lookup("consolas");
        Assert.False(f.IsBundled);
        Assert.Equal("Consolas", f.Source);
    }

    [Fact]
    public void Lookup_unknown_key_falls_back_to_default()
    {
        var f = FontRegistry.Lookup("nonsense");
        Assert.Equal(FontRegistry.DefaultKey, f.Key);
    }

    [Fact]
    public void Each_font_has_a_nonempty_display_name()
    {
        foreach (var f in FontRegistry.All)
            Assert.False(string.IsNullOrWhiteSpace(f.DisplayName));
    }
}
