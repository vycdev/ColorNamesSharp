using color_names_csharp.Utility;
namespace color_names_csharp;
public class NamedColor(string name, short r, short g, short b)
{
    public string Name { get; } = name;
    public (short, short, short) Rgb { get; } = (r, g, b);
    public (float, float, float) Lab => ColorConverter.RGBToLab(Rgb);
    public string Hex => $"#{Rgb.Item1:X2}{Rgb.Item2:X2}{Rgb.Item3:X2}";
}
