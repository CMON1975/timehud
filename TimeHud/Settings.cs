namespace TimeHud;

public sealed class Settings
{
    public double? X { get; set; }
    public double? Y { get; set; }
    public double Opacity { get; set; } = 0.75;
    public double TextOpacity { get; set; } = 1.00;
    public int FontSize { get; set; } = 48;
    public string FontKey { get; set; } = "cascadiamono";
    public string Color { get; set; } = "#00FF5A";
    public int TimerMinutes { get; set; } = 30;
    public bool ShowTimer { get; set; } = true;

    public static Settings Default => new();
}
