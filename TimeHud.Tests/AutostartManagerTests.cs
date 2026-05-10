using TimeHud;

namespace TimeHud.Tests;

public class AutostartManagerTests
{
    private const string RunKey = @"HKCU\Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "TimeHud";

    [Fact]
    public void IsEnabled_false_when_value_missing()
    {
        var registry = new InMemoryRegistryStore();
        var autostart = new AutostartManager(registry);
        Assert.False(autostart.IsEnabled());
    }

    [Fact]
    public void Enable_writes_exe_path_under_run_key()
    {
        var registry = new InMemoryRegistryStore();
        var autostart = new AutostartManager(registry);
        autostart.Enable(@"C:\path\to\TimeHud.exe");
        Assert.Equal(@"C:\path\to\TimeHud.exe", registry.Get(RunKey, ValueName));
    }

    [Fact]
    public void Enable_then_IsEnabled_is_true()
    {
        var registry = new InMemoryRegistryStore();
        var autostart = new AutostartManager(registry);
        autostart.Enable(@"C:\app.exe");
        Assert.True(autostart.IsEnabled());
    }

    [Fact]
    public void Disable_removes_value()
    {
        var registry = new InMemoryRegistryStore();
        registry.Set(RunKey, ValueName, @"C:\app.exe");
        var autostart = new AutostartManager(registry);
        autostart.Disable();
        Assert.False(autostart.IsEnabled());
        Assert.Null(registry.Get(RunKey, ValueName));
    }

    [Fact]
    public void Enable_is_idempotent()
    {
        var registry = new InMemoryRegistryStore();
        var autostart = new AutostartManager(registry);
        autostart.Enable(@"C:\app.exe");
        autostart.Enable(@"C:\app.exe");
        Assert.Equal(@"C:\app.exe", registry.Get(RunKey, ValueName));
    }

    [Fact]
    public void Enable_overwrites_existing_path()
    {
        var registry = new InMemoryRegistryStore();
        var autostart = new AutostartManager(registry);
        autostart.Enable(@"C:\old.exe");
        autostart.Enable(@"D:\new.exe");
        Assert.Equal(@"D:\new.exe", registry.Get(RunKey, ValueName));
    }

    [Fact]
    public void Disable_when_already_disabled_does_not_throw()
    {
        var registry = new InMemoryRegistryStore();
        var autostart = new AutostartManager(registry);
        autostart.Disable();
        autostart.Disable();
        Assert.False(autostart.IsEnabled());
    }
}

internal sealed class InMemoryRegistryStore : IRegistryStore
{
    private readonly Dictionary<string, string> _store = new();

    private static string Key(string keyPath, string valueName) => $"{keyPath}::{valueName}";

    public string? Get(string keyPath, string valueName) =>
        _store.TryGetValue(Key(keyPath, valueName), out var v) ? v : null;

    public void Set(string keyPath, string valueName, string value) =>
        _store[Key(keyPath, valueName)] = value;

    public void Remove(string keyPath, string valueName) =>
        _store.Remove(Key(keyPath, valueName));
}
