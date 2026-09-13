namespace TimeHud;

public enum CyclePhase { Work, Walk }

/// <summary>
/// Two-phase stand-up cycle over a single <see cref="CountdownModel"/>: a Work countdown
/// that, once finished, hands off (on the next press) to a Walk countdown, which in turn
/// hands off (on the next press) to a fresh Work countdown.
/// </summary>
public sealed class StandUpCycle
{
    public const int DefaultWorkMinutes = 30;
    public const int DefaultWalkMinutes = 5;

    private readonly CountdownModel _countdown;

    public CyclePhase Phase { get; private set; } = CyclePhase.Work;
    public int WorkMinutes { get; private set; }
    public int WalkMinutes { get; private set; }
    public CountdownState State => _countdown.State;
    public int RemainingSeconds => _countdown.RemainingSeconds;

    public StandUpCycle(int workMinutes, int walkMinutes)
    {
        WorkMinutes = Math.Clamp(workMinutes, CountdownModel.MinMinutes, CountdownModel.MaxMinutes);
        WalkMinutes = Math.Clamp(walkMinutes, CountdownModel.MinMinutes, CountdownModel.MaxMinutes);
        _countdown = new CountdownModel(WorkMinutes);
    }

    /// <summary>
    /// The single button action. Idle/Running restart the current phase; Finished advances
    /// to the other phase and starts it.
    /// </summary>
    public void Press(DateTime now)
    {
        if (_countdown.State == CountdownState.Finished)
        {
            Phase = Phase == CyclePhase.Work ? CyclePhase.Walk : CyclePhase.Work;
            _countdown.SetDuration(PhaseMinutes, now); // Finished → restarts at the new duration
        }
        else
        {
            _countdown.Start(now);
        }
    }

    /// <summary>Change the work length. Only affects the countdown while in the Work phase.</summary>
    public void SetWorkMinutes(int minutes, DateTime now)
    {
        WorkMinutes = Math.Clamp(minutes, CountdownModel.MinMinutes, CountdownModel.MaxMinutes);
        if (Phase == CyclePhase.Work) _countdown.SetDuration(WorkMinutes, now);
    }

    /// <summary>Change the walk length. Only affects the countdown while in the Walk phase.</summary>
    public void SetWalkMinutes(int minutes, DateTime now)
    {
        WalkMinutes = Math.Clamp(minutes, CountdownModel.MinMinutes, CountdownModel.MaxMinutes);
        if (Phase == CyclePhase.Walk) _countdown.SetDuration(WalkMinutes, now);
    }

    /// <summary>Back to Work/Idle at the full work duration.</summary>
    public void Cancel()
    {
        Phase = CyclePhase.Work;
        _countdown.Cancel();
        _countdown.SetDuration(WorkMinutes, DateTime.MinValue); // Idle: just resets the displayed duration
    }

    public TickEffects Tick(DateTime now) => _countdown.Tick(now);

    private int PhaseMinutes => Phase == CyclePhase.Work ? WorkMinutes : WalkMinutes;
}
