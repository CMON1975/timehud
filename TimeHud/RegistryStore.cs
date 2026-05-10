using Microsoft.Win32;

namespace TimeHud;

public sealed class RegistryStore : IRegistryStore
{
    public string? Get(string keyPath, string valueName)
    {
        var (root, sub) = SplitPath(keyPath);
        using var key = root.OpenSubKey(sub, writable: false);
        return key?.GetValue(valueName) as string;
    }

    public void Set(string keyPath, string valueName, string value)
    {
        var (root, sub) = SplitPath(keyPath);
        using var key = root.CreateSubKey(sub, writable: true);
        key.SetValue(valueName, value);
    }

    public void Remove(string keyPath, string valueName)
    {
        var (root, sub) = SplitPath(keyPath);
        using var key = root.OpenSubKey(sub, writable: true);
        key?.DeleteValue(valueName, throwOnMissingValue: false);
    }

    private static (RegistryKey root, string sub) SplitPath(string keyPath)
    {
        var idx = keyPath.IndexOf('\\');
        if (idx < 0) throw new ArgumentException($"Invalid registry path: {keyPath}");
        var rootName = keyPath[..idx];
        var sub = keyPath[(idx + 1)..];
        var root = rootName switch
        {
            "HKCU" or "HKEY_CURRENT_USER" => Registry.CurrentUser,
            "HKLM" or "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
            _ => throw new ArgumentException($"Unsupported registry root: {rootName}")
        };
        return (root, sub);
    }
}
