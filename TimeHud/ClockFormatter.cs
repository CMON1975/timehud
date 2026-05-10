using System.Globalization;

namespace TimeHud;

public static class ClockFormatter
{
    public static string Format(DateTime now) =>
        now.ToString("yyyy.MM.dd.HH:mm:ss", CultureInfo.InvariantCulture);
}
