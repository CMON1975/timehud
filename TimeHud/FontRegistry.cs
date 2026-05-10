namespace TimeHud;

public sealed record FontEntry(string Key, string DisplayName, bool IsBundled, string Source);

public static class FontRegistry
{
    public const string DefaultKey = "cascadiamono";

    public static readonly IReadOnlyList<FontEntry> All = new[]
    {
        new FontEntry("cascadiamono", "Cascadia Mono", IsBundled: false,
            Source: "Cascadia Mono"),
        new FontEntry("cascadia", "Cascadia Code", IsBundled: false,
            Source: "Cascadia Code"),
        new FontEntry("consolas", "Consolas", IsBundled: false,
            Source: "Consolas"),
        new FontEntry("dseg7", "DSEG7 Classic Mini", IsBundled: true,
            Source: "pack://application:,,,/Fonts/#DSEG7 Classic Mini"),
        new FontEntry("d7mono", "Digital-7 Mono", IsBundled: true,
            Source: "pack://application:,,,/Fonts/#Digital-7 Mono"),
    };

    public static FontEntry Lookup(string key) =>
        All.FirstOrDefault(f => f.Key == key) ?? All.First(f => f.Key == DefaultKey);
}
