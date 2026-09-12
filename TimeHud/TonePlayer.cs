using System.IO;
using System.Media;

namespace TimeHud;

/// <summary>Plays pre-rendered 880 Hz tones through the default audio device via winmm PlaySound.
/// Replaces kernel32 Beep, which is silent in many launch contexts on modern Windows.</summary>
public sealed class TonePlayer : ISoundPlayer
{
    private readonly byte[] _short = WavTone.Generate(880, 150);
    private readonly byte[] _long = WavTone.Generate(880, 1000);

    public void Beep() => Play(_short);

    public void BeepLong() => Play(_long);

    private static void Play(byte[] wav)
    {
        try
        {
            var player = new SoundPlayer(new MemoryStream(wav));
            player.Play(); // asynchronous; the stream is read on a worker thread
        }
        catch
        {
            // Sound is best-effort; never let audio trouble take down the HUD.
        }
    }
}
