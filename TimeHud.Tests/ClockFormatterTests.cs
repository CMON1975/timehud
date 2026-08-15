using System.Globalization;
using TimeHud;

namespace TimeHud.Tests;

public class ClockFormatterTests
{
    [Fact]
    public void Format_uses_date_then_day_then_time()
    {
        var t = new DateTime(2026, 5, 10, 14, 7, 3);
        Assert.Equal("2026.05.10 (SUN) 14:07:03", ClockFormatter.Format(t));
    }

    [Fact]
    public void Format_zero_pads_single_digit_components()
    {
        var t = new DateTime(2001, 1, 2, 3, 4, 5);
        Assert.Equal("2001.01.02 (TUE) 03:04:05", ClockFormatter.Format(t));
    }

    [Theory]
    [InlineData(2026, 8, 10, "MON")]
    [InlineData(2026, 8, 11, "TUE")]
    [InlineData(2026, 8, 12, "WED")]
    [InlineData(2026, 8, 13, "THU")]
    [InlineData(2026, 8, 14, "FRI")]
    [InlineData(2026, 8, 15, "SAT")]
    [InlineData(2026, 8, 16, "SUN")]
    public void Format_uses_uppercase_three_letter_day(int year, int month, int day, string expected)
    {
        var t = new DateTime(year, month, day, 0, 0, 0);
        Assert.Equal($"{year:D4}.{month:D2}.{day:D2} ({expected}) 00:00:00", ClockFormatter.Format(t));
    }

    [Fact]
    public void Format_is_culture_invariant()
    {
        var prior = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("ar-SA");
            var t = new DateTime(2026, 5, 10, 14, 7, 3);
            Assert.Equal("2026.05.10 (SUN) 14:07:03", ClockFormatter.Format(t));
        }
        finally
        {
            CultureInfo.CurrentCulture = prior;
        }
    }
}
