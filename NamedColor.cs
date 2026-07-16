using ColorNamesSharp.Utility;

namespace ColorNamesSharp;
/// <summary>Represents a color with a display name and common color-space values.</summary>
/// <param name="name">The display name of the color.</param>
/// <param name="r">The red channel value from 0 through 255.</param>
/// <param name="g">The green channel value from 0 through 255.</param>
/// <param name="b">The blue channel value from 0 through 255.</param>
public class NamedColor(string name, short r, short g, short b)
{
    /// <summary>Gets the display name of the color.</summary>
    public string Name { get; } = name;
    /// <summary>Gets the red, green, and blue channel values.</summary>
    public (short, short, short) Rgb { get; } = (r, g, b);
    /// <summary>Gets the color converted to the CIELAB color space.</summary>
    public (float, float, float) Lab => ColorConverter.RGBToLab(Rgb);
    /// <summary>Gets the color formatted as <c>#RRGGBB</c>.</summary>
    public string Hex => $"#{Rgb.Item1:X2}{Rgb.Item2:X2}{Rgb.Item3:X2}";
}


