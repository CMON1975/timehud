namespace TimeHud;

public interface IRegistryStore
{
    string? Get(string keyPath, string valueName);
    void Set(string keyPath, string valueName, string value);
    void Remove(string keyPath, string valueName);
}
