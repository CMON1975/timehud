namespace TimeHud;

public interface ISoundPlayer
{
    /// <summary>Short tick for the 3/2/1 countdown.</summary>
    void Beep();

    /// <summary>Long tone when the countdown reaches zero.</summary>
    void BeepLong();
}
