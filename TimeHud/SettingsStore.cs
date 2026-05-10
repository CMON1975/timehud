using System.IO;
using System.Text.Json;

namespace TimeHud;

public static class SettingsStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static Settings Load(string path)
    {
        if (!File.Exists(path)) return Settings.Default;
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Settings>(json) ?? Settings.Default;
        }
        catch (JsonException)
        {
            return Settings.Default;
        }
    }

    public static void Save(string path, Settings settings)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(path, JsonSerializer.Serialize(settings, Options));
    }
}
