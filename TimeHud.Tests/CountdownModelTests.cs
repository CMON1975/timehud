using TimeHud;

namespace TimeHud.Tests;

public class CountdownModelTests
{
    private static readonly DateTime T0 = new(2026, 1, 1, 9, 0, 0);
    private static DateTime At(double seconds) => T0.AddSeconds(seconds);

    [Fact]
    public void Constructor_starts_idle_at_full_duration()
    {
        var m = new CountdownModel(30);
        Assert.Equal(CountdownState.Idle, m.State);
        Assert.Equal(30, m.DurationMinutes);
        Assert.Equal(1800, m.RemainingSeconds);
    }

    [Fact]
    public void Constructor_clamps_below_min()
    {
        Assert.Equal(1, new CountdownModel(0).DurationMinutes);
    }

    [Fact]
    public void Constructor_clamps_above_max()
    {
        Assert.Equal(180, new CountdownModel(999).DurationMinutes);
    }

    [Fact]
    public void Tick_while_idle_is_a_no_op()
    {
        var m = new CountdownModel(30);
        Assert.Equal(TickEffects.None, m.Tick(At(100)));
        Assert.Equal(CountdownState.Idle, m.State);
        Assert.Equal(1800, m.RemainingSeconds);
    }

    [Fact]
    public void Start_enters_running_at_full_duration()
    {
        var m = new CountdownModel(30);
        m.Start(T0);
        Assert.Equal(CountdownState.Running, m.State);
        Assert.Equal(1800, m.RemainingSeconds);
    }

    [Fact]
    public void Tick_uses_ceiling_of_remaining()
    {
        var m = new CountdownModel(30);
        m.Start(T0);
        m.Tick(At(0.5));
        Assert.Equal(1800, m.RemainingSeconds);
        m.Tick(At(1));
        Assert.Equal(1799, m.RemainingSeconds);
        m.Tick(At(1.2));
        Assert.Equal(1799, m.RemainingSeconds);
    }

    [Fact]
    public void Beeps_once_at_each_of_last_three_seconds_then_finishes()
    {
        var m = new CountdownModel(1);
        m.Start(T0);
        Assert.Equal(TickEffects.None, m.Tick(At(56)));   // 4 remaining, no beep
        Assert.Equal(TickEffects.Beep, m.Tick(At(57)));   // 3
        Assert.Equal(TickEffects.None, m.Tick(At(57.2))); // still 3, deduped
        Assert.Equal(TickEffects.Beep, m.Tick(At(58)));   // 2
        Assert.Equal(TickEffects.Beep, m.Tick(At(59)));   // 1
        Assert.Equal(TickEffects.Finished, m.Tick(At(60)));
        Assert.Equal(0, m.RemainingSeconds);
        Assert.Equal(CountdownState.Finished, m.State);
        Assert.Equal(TickEffects.None, m.Tick(At(61)));
        Assert.Equal(CountdownState.Finished, m.State);
    }

    [Fact]
    public void Jump_across_several_seconds_beeps_once()
    {
        var m = new CountdownModel(1);
        m.Start(T0);
        m.Tick(At(55));
        Assert.Equal(TickEffects.Beep, m.Tick(At(59)));
        Assert.Equal(1, m.RemainingSeconds);
    }

    [Fact]
    public void Jump_straight_to_zero_finishes_without_beep()
    {
        var m = new CountdownModel(1);
        m.Start(T0);
        Assert.Equal(TickEffects.Finished, m.Tick(At(75)));
    }

    [Fact]
    public void Start_while_running_restarts_from_full()
    {
        var m = new CountdownModel(1);
        m.Start(T0);
        m.Tick(At(30));
        Assert.Equal(30, m.RemainingSeconds);
        m.Start(At(30));
        Assert.Equal(60, m.RemainingSeconds);
        m.Tick(At(31));
        Assert.Equal(59, m.RemainingSeconds);
    }

    [Fact]
    public void Start_while_finished_restarts()
    {
        var m = new CountdownModel(1);
        m.Start(T0);
        m.Tick(At(60));
        Assert.Equal(CountdownState.Finished, m.State);
        m.Start(At(60));
        Assert.Equal(CountdownState.Running, m.State);
        Assert.Equal(TickEffects.None, m.Tick(At(60.2)));
        Assert.Equal(60, m.RemainingSeconds);
    }

    [Fact]
    public void SetDuration_while_idle_updates_remaining_and_stays_idle()
    {
        var m = new CountdownModel(30);
        m.SetDuration(5, T0);
        Assert.Equal(CountdownState.Idle, m.State);
        Assert.Equal(5, m.DurationMinutes);
        Assert.Equal(300, m.RemainingSeconds);
    }

    [Fact]
    public void SetDuration_while_running_restarts_with_new_duration()
    {
        var m = new CountdownModel(30);
        m.Start(T0);
        m.Tick(At(10));
        m.SetDuration(5, At(10));
        Assert.Equal(CountdownState.Running, m.State);
        Assert.Equal(300, m.RemainingSeconds);
        m.Tick(At(11));
        Assert.Equal(299, m.RemainingSeconds);
    }

    [Fact]
    public void SetDuration_while_finished_restarts()
    {
        var m = new CountdownModel(1);
        m.Start(T0);
        m.Tick(At(60));
        m.SetDuration(5, At(60));
        Assert.Equal(CountdownState.Running, m.State);
        Assert.Equal(300, m.RemainingSeconds);
    }

    [Fact]
    public void Cancel_from_running_returns_to_idle_at_full()
    {
        var m = new CountdownModel(1);
        m.Start(T0);
        m.Tick(At(30));
        m.Cancel();
        Assert.Equal(CountdownState.Idle, m.State);
        Assert.Equal(60, m.RemainingSeconds);
        Assert.Equal(TickEffects.None, m.Tick(At(31)));
    }

    [Fact]
    public void Cancel_from_finished_returns_to_idle_at_full()
    {
        var m = new CountdownModel(1);
        m.Start(T0);
        m.Tick(At(60));
        m.Cancel();
        Assert.Equal(CountdownState.Idle, m.State);
        Assert.Equal(60, m.RemainingSeconds);
    }

    [Fact]
    public void Clock_rollback_clamps_to_full_duration()
    {
        var m = new CountdownModel(1);
        m.Start(T0);
        Assert.Equal(TickEffects.None, m.Tick(At(-10)));
        Assert.Equal(60, m.RemainingSeconds);
        Assert.Equal(CountdownState.Running, m.State);
    }
}
