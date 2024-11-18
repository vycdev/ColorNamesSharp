namespace ColorNames.Utility;
public static class ColorConverter
{
    /// <summary>
    /// Converts RGB color values to LAB color values
    /// </summary>
    /// <param name="RGB">A touple continaing the RGB color values</param>
    /// <returns>A touple containing the LAB color values</returns>
    public static (float, float, float) RGBToLab((short, short, short) RGB)
    {
        float[] xyz = new float[3];
        float[] lab = new float[3];
        float[] rgb = [RGB.Item1 / 255.0f, RGB.Item2 / 255.0f, RGB.Item3 / 255.0f];

        if (rgb[0] > .04045f)
            rgb[0] = (float)Math.Pow((rgb[0] + .055) / 1.055, 2.4);
        else
            rgb[0] = rgb[0] / 12.92f;

        if (rgb[1] > .04045f)
            rgb[1] = (float)Math.Pow((rgb[1] + .055) / 1.055, 2.4);
        else
            rgb[1] = rgb[1] / 12.92f;

        if (rgb[2] > .04045f)
            rgb[2] = (float)Math.Pow((rgb[2] + .055) / 1.055, 2.4);
        else
            rgb[2] = rgb[2] / 12.92f;

        rgb[0] = rgb[0] * 100.0f;
        rgb[1] = rgb[1] * 100.0f;
        rgb[2] = rgb[2] * 100.0f;

        xyz[0] = rgb[0] * .412453f + rgb[1] * .357580f + rgb[2] * .180423f;
        xyz[1] = rgb[0] * .212671f + rgb[1] * .715160f + rgb[2] * .072169f;
        xyz[2] = rgb[0] * .019334f + rgb[1] * .119193f + rgb[2] * .950227f;

        xyz[0] = xyz[0] / 95.047f;
        xyz[1] = xyz[1] / 100.0f;
        xyz[2] = xyz[2] / 108.883f;

        if (xyz[0] > .008856f)
            xyz[0] = (float)Math.Pow(xyz[0], 1.0 / 3.0);
        else
            xyz[0] = xyz[0] * 7.787f + 16.0f / 116.0f;

        if (xyz[1] > .008856f)
            xyz[1] = (float)Math.Pow(xyz[1], 1.0 / 3.0);
        else
            xyz[1] = xyz[1] * 7.787f + 16.0f / 116.0f;

        if (xyz[2] > .008856f)
            xyz[2] = (float)Math.Pow(xyz[2], 1.0 / 3.0);
        else
            xyz[2] = xyz[2] * 7.787f + 16.0f / 116.0f;

        lab[0] = 116.0f * xyz[1] - 16.0f;
        lab[1] = 500.0f * (xyz[0] - xyz[1]);
        lab[2] = 200.0f * (xyz[1] - xyz[2]);

        return (lab[0], lab[1], lab[2]);
    }

    /// <summary>
    /// Converts a hex color value to RGB.
    /// 
    /// Hex string format: #RRGGBB
    /// </summary>
    /// <param name="hex">Hex color string</param>
    /// <returns>A touple containing the rgb values</returns>
    /// <exception cref="ArgumentException">Invalid hex string</exception>
    public static (short r, short g, short b) HexToRgb(string hex)
    {
        if (hex.Length != 7 || hex[0] != '#')
            throw new ArgumentException("Invalid hex string");

        return (
            Convert.ToInt16(hex.Substring(1, 2), 16),
            Convert.ToInt16(hex.Substring(3, 2), 16),
            Convert.ToInt16(hex.Substring(5, 2), 16)
        );
    }
}
