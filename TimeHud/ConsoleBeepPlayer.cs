namespace TimeHud;

/// <summary>Short tone via kernel32 Beep, run off the UI thread so rendering never stalls.</summary>
public sealed class ConsoleBeepPlayer : ISoundPlayer
{
    public void Beep() => Task.Run(() => Console.Beep(880, 150));
}
