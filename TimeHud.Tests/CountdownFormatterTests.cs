using System.Globalization;
using TimeHud;

namespace TimeHud.Tests;

public class CountdownFormatterTests
{
    [Theory]
    [InlineData(0, "00:00")]
    [InlineData(59, "00:59")]
    [InlineData(60, "01:00")]
    [InlineData(1800, "30:00")]
    [InlineData(3600, "60:00")]
    [InlineData(-5, "00:00")]
    public void Format_renders_mm_ss(int seconds, string expected)
    {
        Assert.Equal(expected, CountdownFormatter.Format(seconds));
    }

    [Fact]
    public void Format_is_culture_invariant()
    {
        var prior = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("ar-SA");
            Assert.Equal("05:07", CountdownFormatter.Format(307));
        }
        finally
        {
            CultureInfo.CurrentCulture = prior;
        }
    }
}
