namespace TimeHud;

public enum CountdownState { Idle, Running, Finished }

[Flags]
public enum TickEffects
{
    None = 0,
    Beep = 1,
    Finished = 2,
}

/// <summary>
/// Deadline-based countdown. Remaining time is derived from the deadline on every
/// <see cref="Tick"/>, so the tick interval never affects accuracy or drift.
/// </summary>
public sealed class CountdownModel
{
    public const int MinMinutes = 1;
    public const int MaxMinutes = 180;
    public const int BeepFromSeconds = 3;

    public int DurationMinutes { get; private set; }
    public int DurationSeconds => DurationMinutes * 60;
    public CountdownState State { get; private set; } = CountdownState.Idle;
    public int RemainingSeconds { get; private set; }

    private DateTime _deadline;

    public CountdownModel(int minutes)
    {
        DurationMinutes = Math.Clamp(minutes, MinMinutes, MaxMinutes);
        RemainingSeconds = DurationSeconds;
    }

    /// <summary>Start (or restart) from the full duration. Valid in any state.</summary>
    public void Start(DateTime now)
    {
        _deadline = now.AddSeconds(DurationSeconds);
        RemainingSeconds = DurationSeconds;
        State = CountdownState.Running;
    }

    /// <summary>
    /// Change the duration. Idle stays idle showing the new duration; Running or
    /// Finished restart from the new duration.
    /// </summary>
    public void SetDuration(int minutes, DateTime now)
    {
        DurationMinutes = Math.Clamp(minutes, MinMinutes, MaxMinutes);
        if (State == CountdownState.Idle)
            RemainingSeconds = DurationSeconds;
        else
            Start(now);
    }

    /// <summary>Return to Idle at the full duration. Valid in any state.</summary>
    public void Cancel()
    {
        State = CountdownState.Idle;
        RemainingSeconds = DurationSeconds;
    }

    public TickEffects Tick(DateTime now)
    {
        if (State != CountdownState.Running) return TickEffects.None;

        var raw = (int)Math.Ceiling((_deadline - now).TotalSeconds);
        var remaining = Math.Clamp(raw, 0, DurationSeconds);
        var previous = RemainingSeconds;
        RemainingSeconds = remaining;

        var effects = TickEffects.None;
        if (remaining is >= 1 and <= BeepFromSeconds && remaining != previous)
            effects |= TickEffects.Beep;

        if (remaining == 0)
        {
            State = CountdownState.Finished;
            effects |= TickEffects.Finished;
        }
        return effects;
    }
}
