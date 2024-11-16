using color_names_csharp.Utility;

namespace color_names_csharp;
public class ColorNamesBuilder
{
    /// <summary>
    /// List of named colors.
    /// </summary>
    public List<NamedColor> NamedColors { get; } = [];

    /// <summary>
    /// Builds a ColorNames object from the list of named colors.
    /// </summary>
    public ColorNames BuildColorNames => new(NamedColors);

    // TODO: make add color methods 

    /// <summary>
    /// Adds the colors from a CSV file to the list of named colors in this builder.
    /// 
    /// The CSV file format should be: name, hex
    /// The first line of the CSV file is ignored.
    /// </summary>
    /// <param name="path">Path to CSV file</param>
    public ColorNamesBuilder AddFromCsv(string path)
    {
        string[] lines = File.ReadAllLines(path);

        for (int i = 1; i < lines.Length; i++)
        {
            string? line = lines[i];
            string[] parts = line.Split(',');
            string hex = parts[1];
            (short r, short g, short b) = ColorConverter.HexToRgb(hex);
            NamedColors.Add(new NamedColor(parts[0], r, g, b));
        }

        return this; 
    }

    /// <summary>
    /// Load the default CSV with the color names. 
    /// 
    /// Calling this function multiple times will add the default color names multiple times to the list. 
    /// </summary>
    public ColorNamesBuilder LoadDefault() 
        => AddFromCsv(Path.Combine(Environment.CurrentDirectory, "Resources\\colornames.csv"));

}
