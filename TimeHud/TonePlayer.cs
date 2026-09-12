using System.IO;
using System.Media;

namespace TimeHud;

/// <summary>Plays a pre-rendered 880 Hz tone through the default audio device via winmm PlaySound.
/// Replaces kernel32 Beep, which is silent in many launch contexts on modern Windows.</summary>
public sealed class TonePlayer : ISoundPlayer
{
    private readonly byte[] _wav = WavTone.Generate(880, 150);

    public void Beep()
    {
        try
        {
            var player = new SoundPlayer(new MemoryStream(_wav));
            player.Play(); // asynchronous; the stream is read on a worker thread
        }
        catch
        {
            // Sound is best-effort; never let audio trouble take down the HUD.
        }
    }
}
