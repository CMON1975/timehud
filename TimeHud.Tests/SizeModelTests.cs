using TimeHud;

namespace TimeHud.Tests;

public class SizeModelTests
{
    [Fact]
    public void Default_starts_at_constructor_value()
    {
        var m = new SizeModel(48);
        Assert.Equal(48, m.Value);
    }

    [Fact]
    public void Constructor_clamps_above_max()
    {
        var m = new SizeModel(500);
        Assert.Equal(200, m.Value);
    }

    [Fact]
    public void Constructor_clamps_below_min()
    {
        var m = new SizeModel(4);
        Assert.Equal(16, m.Value);
    }

    [Fact]
    public void StepUp_adds_4pt()
    {
        var m = new SizeModel(48);
        m.StepUp();
        Assert.Equal(52, m.Value);
    }

    [Fact]
    public void StepDown_subtracts_4pt()
    {
        var m = new SizeModel(48);
        m.StepDown();
        Assert.Equal(44, m.Value);
    }

    [Fact]
    public void StepUp_at_max_is_idempotent()
    {
        var m = new SizeModel(200);
        m.StepUp();
        m.StepUp();
        Assert.Equal(200, m.Value);
    }

    [Fact]
    public void StepDown_at_min_is_idempotent()
    {
        var m = new SizeModel(16);
        m.StepDown();
        m.StepDown();
        Assert.Equal(16, m.Value);
    }
}
