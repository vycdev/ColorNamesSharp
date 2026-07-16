using ColorNamesSharp.Utility;
using System.Reflection;

namespace ColorNamesSharp;
/// <summary>Builds a <see cref="ColorNames"/> instance from bundled, custom, or CSV color data.</summary>
public class ColorNamesBuilder
{
    /// <summary>
    /// List of named colors.
    /// </summary>
    public List<NamedColor> NamedColors { get; } = [];

    /// <summary>
    /// Builds a <see cref="ColorNames"/> instance from the current color list.
    /// </summary>
    /// <returns>A lookup instance containing the configured colors.</returns>
    public ColorNames Build() 
        => new(NamedColors);

    /// <summary>
    /// Adds a named color to the list of named colors in this builder.
    /// </summary>
    /// <param name="name">The display name of the color.</param>
    /// <param name="r">The red channel value from 0 through 255.</param>
    /// <param name="g">The green channel value from 0 through 255.</param>
    /// <param name="b">The blue channel value from 0 through 255.</param>
    /// <returns>This builder, so additional calls can be chained.</returns>
    public ColorNamesBuilder Add(string name, short r, short g, short b)
    {
        NamedColors.Add(new NamedColor(name, r, g, b));
        return this;
    }

    /// <summary>
    /// Adds a named color to the list of named colors in this builder.
    /// </summary>
    /// <param name="name">The display name of the color.</param>
    /// <param name="hex">The color value in <c>#RRGGBB</c> format.</param>
    /// <returns>This builder, so additional calls can be chained.</returns>
    /// <exception cref="ArgumentException"><paramref name="hex"/> is not a six-digit hexadecimal color.</exception>
    public ColorNamesBuilder Add(string name, string hex)
    {
        (short r, short g, short b) = ColorConverter.HexToRgb(hex);
        NamedColors.Add(new NamedColor(name, r, g, b));
        return this;
    }

    /// <summary>
    /// Adds a named color to the list of named colors in this builder.
    /// </summary>
    /// <param name="color">The color to add.</param>
    /// <returns>This builder, so additional calls can be chained.</returns>
    public ColorNamesBuilder Add(NamedColor color)
    {
        NamedColors.Add(color);
        return this;
    }

    /// <summary>
    /// Adds the colors from a CSV file lines to the list of named colors in this builder.
    /// </summary>
    /// <param name="lines"></param>
    /// <returns></returns>
    private ColorNamesBuilder AddColorsFromLines(string[] lines)
    {
        for (int i = 1; i < lines.Length; i++)
        {
            string? line = lines[i];

            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split(',');
            string hex = parts[1];
            (short r, short g, short b) = ColorConverter.HexToRgb(hex);
            NamedColors.Add(new NamedColor(parts[0], r, g, b));
        }

        return this;
    }

    /// <summary>
    /// Adds the colors from a CSV file to the list of named colors in this builder.
    /// 
    /// The CSV file format should be: name, hex
    /// The first line of the CSV file is ignored.
    /// </summary>
    /// <param name="path">The path to a CSV file whose header is followed by <c>name,hex</c> rows.</param>
    /// <returns>This builder, so additional calls can be chained.</returns>
    public ColorNamesBuilder AddFromCsv(string path)
    {
        string[] lines = File.ReadAllLines(path);

        return AddColorsFromLines(lines);
    }

    /// <summary>
    /// Load the default CSV with the color names. 
    /// 
    /// Calling this function multiple times will add the default color names multiple times to the list. 
    /// </summary>
    /// <returns>This builder, so additional calls can be chained.</returns>
    public ColorNamesBuilder LoadDefault()
    {
        using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ColorNamesSharp.ColorLists.Default.csv");

        if (stream == null)
            throw new Exception("Could not load default color names CSV file.");

        using StreamReader reader = new(stream);
        string[] lines = reader.ReadToEnd().Split('\n');

        _ = AddColorsFromLines(lines);

        reader.Dispose();
        stream.Dispose();

        return this;
    }

}
