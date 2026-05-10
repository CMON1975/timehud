using System.Globalization;
using TimeHud;

namespace TimeHud.Tests;

public class ClockFormatterTests
{
    [Fact]
    public void Format_uses_yyyyMMddHHmmss_with_dots_and_colons()
    {
        var t = new DateTime(2026, 5, 10, 14, 7, 3);
        Assert.Equal("2026.05.10.14:07:03", ClockFormatter.Format(t));
    }

    [Fact]
    public void Format_zero_pads_single_digit_components()
    {
        var t = new DateTime(2001, 1, 2, 3, 4, 5);
        Assert.Equal("2001.01.02.03:04:05", ClockFormatter.Format(t));
    }

    [Fact]
    public void Format_is_culture_invariant()
    {
        var prior = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("ar-SA");
            var t = new DateTime(2026, 5, 10, 14, 7, 3);
            Assert.Equal("2026.05.10.14:07:03", ClockFormatter.Format(t));
        }
        finally
        {
            CultureInfo.CurrentCulture = prior;
        }
    }
}
