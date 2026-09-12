namespace TimeHud;

/// <summary>Builds an in-memory 16-bit mono PCM WAV containing a sine tone with short fade-in/out edges.</summary>
public static class WavTone
{
    public const int SampleRate = 44100;

    public static byte[] Generate(double frequencyHz, int durationMs, double amplitude = 0.5)
    {
        if (frequencyHz <= 0) throw new ArgumentOutOfRangeException(nameof(frequencyHz));
        if (durationMs <= 0) throw new ArgumentOutOfRangeException(nameof(durationMs));
        amplitude = Math.Clamp(amplitude, 0, 1);

        int samples = SampleRate * durationMs / 1000;
        int fade = Math.Min(samples / 2, SampleRate / 200); // 5 ms edges avoid clicks
        int dataBytes = samples * 2;

        var buf = new byte[44 + dataBytes];
        var span = buf.AsSpan();
        WriteAscii(span, 0, "RIFF");
        BitConverter.TryWriteBytes(span[4..], 36 + dataBytes);
        WriteAscii(span, 8, "WAVE");
        WriteAscii(span, 12, "fmt ");
        BitConverter.TryWriteBytes(span[16..], 16);
        BitConverter.TryWriteBytes(span[20..], (short)1);           // PCM
        BitConverter.TryWriteBytes(span[22..], (short)1);           // mono
        BitConverter.TryWriteBytes(span[24..], SampleRate);
        BitConverter.TryWriteBytes(span[28..], SampleRate * 2);     // byte rate
        BitConverter.TryWriteBytes(span[32..], (short)2);           // block align
        BitConverter.TryWriteBytes(span[34..], (short)16);          // bits per sample
        WriteAscii(span, 36, "data");
        BitConverter.TryWriteBytes(span[40..], dataBytes);

        for (int i = 0; i < samples; i++)
        {
            double env = 1.0;
            if (i < fade) env = (double)i / fade;
            else if (i >= samples - fade) env = (double)(samples - 1 - i) / fade;
            double v = amplitude * env * Math.Sin(2 * Math.PI * frequencyHz * i / SampleRate);
            BitConverter.TryWriteBytes(span[(44 + i * 2)..], (short)(v * short.MaxValue));
        }
        return buf;
    }

    private static void WriteAscii(Span<byte> span, int offset, string s)
    {
        for (int i = 0; i < s.Length; i++) span[offset + i] = (byte)s[i];
    }
}
