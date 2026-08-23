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

    // Snapshot the on-disk file to <path>.bak so the pre-launch state survives
    // anything this run does to the live file (wheel spam, corrupt-file reset).
    public static void Archive(string path)
    {
        if (!File.Exists(path)) return;
        File.Copy(path, path + ".bak", overwrite: true);
    }

    public static void Save(string path, Settings settings)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(path, JsonSerializer.Serialize(settings, Options));
    }
}
