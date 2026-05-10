namespace TimeHud;

public sealed class AutostartManager
{
    public const string RunKey = @"HKCU\Software\Microsoft\Windows\CurrentVersion\Run";
    public const string ValueName = "TimeHud";

    private readonly IRegistryStore _registry;

    public AutostartManager(IRegistryStore registry)
    {
        _registry = registry;
    }

    public bool IsEnabled() => _registry.Get(RunKey, ValueName) is not null;

    public void Enable(string exePath) => _registry.Set(RunKey, ValueName, exePath);

    public void Disable() => _registry.Remove(RunKey, ValueName);
}
