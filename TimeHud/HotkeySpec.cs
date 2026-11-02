using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Input;

namespace TimeHud;

/// <summary>
/// A global hotkey as written in settings.json, e.g. <c>"Ctrl+Alt+W"</c>: '+'-separated modifiers
/// (Ctrl/Control, Alt, Shift, Win/Windows) and exactly one key, in any order, case and spaces
/// ignored. At least one modifier is required, since a bare key would be swallowed in every app.
/// The key is a WPF <see cref="Key"/> name (W, Space, F12, NumPad5, OemPlus...); a single digit
/// means the top-row key.
/// </summary>
public sealed record HotkeySpec(ModifierKeys Modifiers, Key Key)
{
    public static readonly HotkeySpec Default = new(ModifierKeys.Control | ModifierKeys.Alt, Key.W);

    public static bool TryParse(string? text, [NotNullWhen(true)] out HotkeySpec? spec)
    {
        spec = null;
        if (string.IsNullOrWhiteSpace(text)) return false;

        var modifiers = ModifierKeys.None;
        Key? key = null;
        foreach (var raw in text.Split('+'))
        {
            var part = raw.Trim();
            var modifier = ModifierFor(part);
            if (modifier != ModifierKeys.None)
            {
                modifiers |= modifier;
                continue;
            }
            if (key is not null || !TryParseKey(part, out var k)) return false;
            key = k;
        }

        if (modifiers == ModifierKeys.None || key is null) return false;
        spec = new HotkeySpec(modifiers, key.Value);
        return true;
    }

    private static ModifierKeys ModifierFor(string part) => part.ToLowerInvariant() switch
    {
        "ctrl" or "control" => ModifierKeys.Control,
        "alt" => ModifierKeys.Alt,
        "shift" => ModifierKeys.Shift,
        "win" or "windows" => ModifierKeys.Windows,
        _ => ModifierKeys.None,
    };

    // Keys that can't be the hotkey's key: modifiers themselves, and None.
    private static readonly HashSet<Key> NotAKey =
    [
        Key.None, Key.System,
        Key.LeftCtrl, Key.RightCtrl, Key.LeftAlt, Key.RightAlt,
        Key.LeftShift, Key.RightShift, Key.LWin, Key.RWin,
    ];

    // Names only: Enum.TryParse would also accept a number ("44") or a comma list ("W,X").
    private static bool TryParseKey(string part, out Key key)
    {
        key = Key.None;
        if (part.Length == 1 && char.IsAsciiDigit(part[0])) part = "D" + part;
        return part.Length > 0
            && char.IsAsciiLetter(part[0])
            && part.All(char.IsAsciiLetterOrDigit)
            && Enum.TryParse(part, ignoreCase: true, out key)
            && !NotAKey.Contains(key);
    }

    // Canonical order Ctrl+Alt+Shift+Win+Key, with top-row digits shown as the digit.
    public override string ToString()
    {
        var parts = new List<string>();
        if (Modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (Modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
        if (Modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
        if (Modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
        parts.Add(Key is >= Key.D0 and <= Key.D9
            ? (Key - Key.D0).ToString(CultureInfo.InvariantCulture)
            : Key.ToString());
        return string.Join("+", parts);
    }
}
