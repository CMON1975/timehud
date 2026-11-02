using System.Windows.Input;
using TimeHud;

namespace TimeHud.Tests;

public class HotkeySpecTests
{
    [Theory]
    [InlineData("Ctrl+Alt+W", ModifierKeys.Control | ModifierKeys.Alt, Key.W)]
    [InlineData(" ctrl + ALT + w ", ModifierKeys.Control | ModifierKeys.Alt, Key.W)]
    [InlineData("w+alt+ctrl", ModifierKeys.Control | ModifierKeys.Alt, Key.W)]
    [InlineData("Control+Shift+Win+F12", ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Windows, Key.F12)]
    [InlineData("Windows+Space", ModifierKeys.Windows, Key.Space)]
    [InlineData("Ctrl+Alt+1", ModifierKeys.Control | ModifierKeys.Alt, Key.D1)]
    public void TryParse_reads_modifiers_and_key(string text, ModifierKeys modifiers, Key key)
    {
        Assert.True(HotkeySpec.TryParse(text, out var spec));
        Assert.Equal(new HotkeySpec(modifiers, key), spec);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("W")]                 // no modifier: would swallow the key in every app
    [InlineData("Ctrl+Alt")]          // no key
    [InlineData("Ctrl+W+X")]          // two keys
    [InlineData("Ctrl++W")]           // empty part
    [InlineData("Ctrl+Foo")]          // unknown key name
    [InlineData("Ctrl+44")]           // Enum.TryParse would take the number
    [InlineData("Ctrl+W,X")]          // Enum.TryParse would take the comma list
    [InlineData("Ctrl+Alt+LeftShift")] // a modifier key can't be the key
    [InlineData("Ctrl+None")]
    public void TryParse_rejects_invalid_text(string? text)
    {
        Assert.False(HotkeySpec.TryParse(text, out _));
    }

    [Theory]
    [InlineData("w+win+shift+alt+ctrl", "Ctrl+Alt+Shift+Win+W")]
    [InlineData("alt+ctrl+1", "Ctrl+Alt+1")]
    [InlineData("control+f12", "Ctrl+F12")]
    public void ToString_is_canonical(string text, string label)
    {
        Assert.True(HotkeySpec.TryParse(text, out var spec));
        Assert.Equal(label, spec!.ToString());
    }

    [Fact]
    public void Default_is_ctrl_alt_w_and_matches_the_settings_default()
    {
        Assert.Equal(new HotkeySpec(ModifierKeys.Control | ModifierKeys.Alt, Key.W), HotkeySpec.Default);
        Assert.True(HotkeySpec.TryParse(Settings.Default.Hotkey, out var fromSettings));
        Assert.Equal(HotkeySpec.Default, fromSettings);
    }
}
