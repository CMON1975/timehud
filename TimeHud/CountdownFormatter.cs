using System.Globalization;

namespace TimeHud;

public static class CountdownFormatter
{
    /// <summary>Formats whole seconds as mm:ss (minutes may exceed 59). Negative clamps to 00:00.</summary>
    public static string Format(int seconds)
    {
        seconds = Math.Max(0, seconds);
        return string.Create(CultureInfo.InvariantCulture, $"{seconds / 60:00}:{seconds % 60:00}");
    }
}
