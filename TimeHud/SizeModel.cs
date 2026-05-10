namespace TimeHud;

public sealed class SizeModel
{
    public const int Min = 16;
    public const int Max = 200;
    public const int Step = 4;

    public int Value { get; private set; }

    public SizeModel(int initial)
    {
        Value = Math.Clamp(initial, Min, Max);
    }

    public void StepUp() => Value = Math.Clamp(Value + Step, Min, Max);
    public void StepDown() => Value = Math.Clamp(Value - Step, Min, Max);
}
