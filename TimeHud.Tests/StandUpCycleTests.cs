using TimeHud;

namespace TimeHud.Tests;

public class StandUpCycleTests
{
    private static readonly DateTime T0 = new(2026, 1, 1, 9, 0, 0);
    private static DateTime At(double seconds) => T0.AddSeconds(seconds);

    private static StandUpCycle FinishedWork()
    {
        var c = new StandUpCycle(1, 2);
        c.Press(T0);
        c.Tick(At(60));
        return c;
    }

    private static StandUpCycle FinishedWalk()
    {
        var c = FinishedWork();
        c.Press(At(60));
        c.Tick(At(180));
        return c;
    }

    [Fact]
    public void Constructor_starts_work_idle_at_full_work_duration()
    {
        var c = new StandUpCycle(30, 5);
        Assert.Equal(CyclePhase.Work, c.Phase);
        Assert.Equal(CountdownState.Idle, c.State);
        Assert.Equal(30, c.WorkMinutes);
        Assert.Equal(5, c.WalkMinutes);
        Assert.Equal(1800, c.RemainingSeconds);
    }

    [Fact]
    public void Constructor_clamps_both_lengths()
    {
        var c = new StandUpCycle(0, 999);
        Assert.Equal(1, c.WorkMinutes);
        Assert.Equal(180, c.WalkMinutes);
    }

    [Fact]
    public void Press_from_work_idle_starts_work()
    {
        var c = new StandUpCycle(1, 2);
        c.Press(T0);
        Assert.Equal(CyclePhase.Work, c.Phase);
        Assert.Equal(CountdownState.Running, c.State);
        Assert.Equal(60, c.RemainingSeconds);
    }

    [Fact]
    public void Press_while_work_running_restarts_work()
    {
        var c = new StandUpCycle(1, 2);
        c.Press(T0);
        c.Tick(At(30));
        c.Press(At(30));
        Assert.Equal(CyclePhase.Work, c.Phase);
        Assert.Equal(60, c.RemainingSeconds);
        c.Tick(At(31));
        Assert.Equal(59, c.RemainingSeconds);
    }

    [Fact]
    public void Work_finish_reports_finished_and_stays_in_work_phase()
    {
        var c = new StandUpCycle(1, 2);
        c.Press(T0);
        Assert.Equal(TickEffects.Finished, c.Tick(At(60)));
        Assert.Equal(CyclePhase.Work, c.Phase);
        Assert.Equal(CountdownState.Finished, c.State);
    }

    [Fact]
    public void Press_after_work_finished_starts_walk()
    {
        var c = FinishedWork();
        c.Press(At(60));
        Assert.Equal(CyclePhase.Walk, c.Phase);
        Assert.Equal(CountdownState.Running, c.State);
        Assert.Equal(120, c.RemainingSeconds);
        c.Tick(At(61));
        Assert.Equal(119, c.RemainingSeconds);
    }

    [Fact]
    public void Press_while_walk_running_restarts_walk()
    {
        var c = FinishedWork();
        c.Press(At(60));
        c.Tick(At(100));
        c.Press(At(100));
        Assert.Equal(CyclePhase.Walk, c.Phase);
        Assert.Equal(120, c.RemainingSeconds);
    }

    [Fact]
    public void Walk_beeps_and_finishes_like_work()
    {
        var c = FinishedWork();
        c.Press(At(60));
        Assert.Equal(TickEffects.None, c.Tick(At(176)));
        Assert.Equal(TickEffects.Beep, c.Tick(At(177)));
        Assert.Equal(TickEffects.Beep, c.Tick(At(178)));
        Assert.Equal(TickEffects.Beep, c.Tick(At(179)));
        Assert.Equal(TickEffects.Finished, c.Tick(At(180)));
        Assert.Equal(CyclePhase.Walk, c.Phase);
        Assert.Equal(CountdownState.Finished, c.State);
    }

    [Fact]
    public void Press_after_walk_finished_starts_next_work()
    {
        var c = FinishedWalk();
        c.Press(At(180));
        Assert.Equal(CyclePhase.Work, c.Phase);
        Assert.Equal(CountdownState.Running, c.State);
        Assert.Equal(60, c.RemainingSeconds);
    }

    [Fact]
    public void SetWorkMinutes_while_idle_updates_display_and_stays_idle()
    {
        var c = new StandUpCycle(30, 5);
        c.SetWorkMinutes(10, T0);
        Assert.Equal(CountdownState.Idle, c.State);
        Assert.Equal(600, c.RemainingSeconds);
    }

    [Fact]
    public void SetWorkMinutes_while_work_running_restarts()
    {
        var c = new StandUpCycle(1, 2);
        c.Press(T0);
        c.Tick(At(30));
        c.SetWorkMinutes(5, At(30));
        Assert.Equal(CountdownState.Running, c.State);
        Assert.Equal(300, c.RemainingSeconds);
    }

    [Fact]
    public void SetWorkMinutes_during_walk_does_not_disturb_walk()
    {
        var c = FinishedWork();
        c.Press(At(60));
        c.Tick(At(70));
        c.SetWorkMinutes(5, At(70));
        Assert.Equal(CyclePhase.Walk, c.Phase);
        Assert.Equal(5, c.WorkMinutes);
        Assert.Equal(110, c.RemainingSeconds);
        c.Tick(At(180));
        c.Press(At(180));
        Assert.Equal(300, c.RemainingSeconds); // next work uses the new length
    }

    [Fact]
    public void SetWalkMinutes_during_work_does_not_disturb_work()
    {
        var c = new StandUpCycle(1, 2);
        c.Press(T0);
        c.Tick(At(10));
        c.SetWalkMinutes(3, At(10));
        Assert.Equal(CyclePhase.Work, c.Phase);
        Assert.Equal(3, c.WalkMinutes);
        Assert.Equal(50, c.RemainingSeconds);
        c.Tick(At(60));
        c.Press(At(60));
        Assert.Equal(180, c.RemainingSeconds); // walk uses the new length
    }

    [Fact]
    public void SetWalkMinutes_during_walk_restarts_walk()
    {
        var c = FinishedWork();
        c.Press(At(60));
        c.Tick(At(70));
        c.SetWalkMinutes(1, At(70));
        Assert.Equal(CountdownState.Running, c.State);
        Assert.Equal(60, c.RemainingSeconds);
    }

    [Fact]
    public void Cancel_from_walk_finished_returns_to_work_idle()
    {
        var c = FinishedWalk();
        c.Cancel();
        Assert.Equal(CyclePhase.Work, c.Phase);
        Assert.Equal(CountdownState.Idle, c.State);
        Assert.Equal(60, c.RemainingSeconds);
        Assert.Equal(TickEffects.None, c.Tick(At(200)));
    }

    [Fact]
    public void Cancel_from_walk_running_returns_to_work_idle()
    {
        var c = FinishedWork();
        c.Press(At(60));
        c.Cancel();
        Assert.Equal(CyclePhase.Work, c.Phase);
        Assert.Equal(CountdownState.Idle, c.State);
        Assert.Equal(60, c.RemainingSeconds);
    }
}
