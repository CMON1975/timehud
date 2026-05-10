using TimeHud;

namespace TimeHud.Tests;

public class OpacityModelTests
{
    [Fact]
    public void Default_starts_at_constructor_value()
    {
        var m = new OpacityModel(0.75);
        Assert.Equal(0.75, m.Value, 3);
    }

    [Fact]
    public void Constructor_clamps_above_max()
    {
        var m = new OpacityModel(2.0);
        Assert.Equal(1.0, m.Value, 3);
    }

    [Fact]
    public void Constructor_clamps_below_min()
    {
        var m = new OpacityModel(0.0);
        Assert.Equal(0.10, m.Value, 3);
    }

    [Fact]
    public void StepUp_adds_one_step()
    {
        var m = new OpacityModel(0.50);
        m.StepUp();
        Assert.Equal(0.55, m.Value, 3);
    }

    [Fact]
    public void StepDown_subtracts_one_step()
    {
        var m = new OpacityModel(0.50);
        m.StepDown();
        Assert.Equal(0.45, m.Value, 3);
    }

    [Fact]
    public void StepUp_at_max_is_idempotent()
    {
        var m = new OpacityModel(1.0);
        m.StepUp();
        m.StepUp();
        Assert.Equal(1.0, m.Value, 3);
    }

    [Fact]
    public void StepDown_at_min_is_idempotent()
    {
        var m = new OpacityModel(0.10);
        m.StepDown();
        m.StepDown();
        Assert.Equal(0.10, m.Value, 3);
    }

    [Fact]
    public void Many_steps_up_then_down_returns_close_to_start()
    {
        var m = new OpacityModel(0.50);
        for (int i = 0; i < 4; i++) m.StepUp();
        for (int i = 0; i < 4; i++) m.StepDown();
        Assert.Equal(0.50, m.Value, 3);
    }
}
