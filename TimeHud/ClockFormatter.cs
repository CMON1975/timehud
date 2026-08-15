using System.Globalization;

namespace TimeHud;

public static class ClockFormatter
{
    public static string Format(DateTime now)
    {
        var date = now.ToString("yyyy.MM.dd", CultureInfo.InvariantCulture);
        var day = now.ToString("ddd", CultureInfo.InvariantCulture).ToUpperInvariant();
        var time = now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        return $"{date} ({day}) {time}";
    }
}
