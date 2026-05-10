using SwmFontFamily = System.Windows.Media.FontFamily;

namespace TimeHud;

public static class FontFamilyFactory
{
    public static SwmFontFamily For(FontEntry entry) => new(entry.Source);
}
