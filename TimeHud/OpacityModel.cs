namespace TimeHud;

public sealed class OpacityModel
{
    public const double Min = 0.10;
    public const double Max = 1.00;
    public const double Step = 0.05;

    public double Value { get; private set; }

    public OpacityModel(double initial)
    {
        Value = Math.Clamp(initial, Min, Max);
    }

    public void StepUp() => Value = Math.Clamp(Value + Step, Min, Max);
    public void StepDown() => Value = Math.Clamp(Value - Step, Min, Max);
}
