namespace TimeHud;

public sealed record ColorPreset(string Key, string DisplayName, string Hex);

public static class ColorPalette
{
    public const string DefaultKey = "green";

    public static readonly IReadOnlyList<ColorPreset> All = new[]
    {
        new ColorPreset("green", "Phosphor Green", "#00FF5A"),
        new ColorPreset("amber", "Amber", "#FFB000"),
        new ColorPreset("cyan", "Cyan", "#00E5FF"),
        new ColorPreset("white", "White", "#F0F0F0"),
        new ColorPreset("red", "Red", "#FF3B30"),
    };

    public static ColorPreset Lookup(string key) =>
        All.FirstOrDefault(c => c.Key == key) ?? All.First(c => c.Key == DefaultKey);
}
