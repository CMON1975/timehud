using TimeHud;

namespace TimeHud.Tests;

public class WavToneTests
{
    [Fact]
    public void Header_Is_Valid_Mono_16Bit_Pcm()
    {
        var wav = WavTone.Generate(880, 150);
        Assert.Equal("RIFF", System.Text.Encoding.ASCII.GetString(wav, 0, 4));
        Assert.Equal("WAVE", System.Text.Encoding.ASCII.GetString(wav, 8, 4));
        Assert.Equal("fmt ", System.Text.Encoding.ASCII.GetString(wav, 12, 4));
        Assert.Equal("data", System.Text.Encoding.ASCII.GetString(wav, 36, 4));
        Assert.Equal(1, BitConverter.ToInt16(wav, 20));   // PCM
        Assert.Equal(1, BitConverter.ToInt16(wav, 22));   // mono
        Assert.Equal(WavTone.SampleRate, BitConverter.ToInt32(wav, 24));
        Assert.Equal(16, BitConverter.ToInt16(wav, 34));
    }

    [Fact]
    public void Sizes_Match_Duration()
    {
        var wav = WavTone.Generate(880, 150);
        int samples = WavTone.SampleRate * 150 / 1000;
        Assert.Equal(44 + samples * 2, wav.Length);
        Assert.Equal(samples * 2, BitConverter.ToInt32(wav, 40));
        Assert.Equal(wav.Length - 8, BitConverter.ToInt32(wav, 4));
    }

    [Fact]
    public void Tone_Is_Audible_And_Starts_And_Ends_Silent()
    {
        var wav = WavTone.Generate(880, 150);
        Assert.Equal(0, BitConverter.ToInt16(wav, 44));
        Assert.Equal(0, BitConverter.ToInt16(wav, wav.Length - 2));
        int peak = 0;
        for (int i = 44; i < wav.Length; i += 2) peak = Math.Max(peak, Math.Abs((int)BitConverter.ToInt16(wav, i)));
        Assert.InRange(peak, short.MaxValue * 0.45, short.MaxValue * 0.5);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(880, 0)]
    public void Rejects_Invalid_Args(double hz, int ms)
        => Assert.Throws<ArgumentOutOfRangeException>(() => WavTone.Generate(hz, ms));
}
