using ColorNamesSharp.Tree;
using ColorNamesSharp.Utility;

namespace ColorNamesSharp;

public class ColorNames
{
    private readonly NamedColor[] namedColors;
    private readonly Dictionary<string, NamedColor> colorsByName;
    private readonly Dictionary<string, NamedColor> colorsByHex;

    /// <summary>
    /// The configured colors in their original insertion order.
    /// </summary>
    public IReadOnlyList<NamedColor> Colors { get; }

    public KDNode? ColorTreeRoot { get; }

    public ColorNames(IEnumerable<NamedColor> namedColors)
    {
        ArgumentNullException.ThrowIfNull(namedColors);

        this.namedColors = namedColors.ToArray();
        Colors = Array.AsReadOnly(this.namedColors);
        ColorTreeRoot = KDTreeBuilder.Build(this.namedColors.ToList(), 0);
        colorsByName = new Dictionary<string, NamedColor>(StringComparer.OrdinalIgnoreCase);
        colorsByHex = new Dictionary<string, NamedColor>(StringComparer.OrdinalIgnoreCase);

        foreach (NamedColor color in this.namedColors)
        {
            _ = colorsByName.TryAdd(color.Name, color);
            _ = colorsByHex.TryAdd(color.Hex, color);
        }
    }

    /// <summary>
    /// Tries to get an exact color by name using a case-insensitive lookup.
    /// If duplicate names are configured, the first color is returned.
    /// </summary>
    /// <param name="name">The color name.</param>
    /// <param name="color">The matching color, or null when no match exists.</param>
    /// <returns>True when a matching color exists; otherwise, false.</returns>
    public bool TryGetByName(string? name, out NamedColor? color)
    {
        if (name == null)
        {
            color = null;
            return false;
        }

        return colorsByName.TryGetValue(name, out color);
    }

    /// <summary>
    /// Tries to get an exact color by six-digit hex value using a case-insensitive lookup.
    /// If duplicate values are configured, the first color is returned.
    /// </summary>
    /// <param name="hex">The color value in #RRGGBB format.</param>
    /// <param name="color">The matching color, or null when no match exists.</param>
    /// <returns>True when a matching color exists; otherwise, false.</returns>
    public bool TryGetByHex(string? hex, out NamedColor? color)
    {
        if (hex == null)
        {
            color = null;
            return false;
        }

        return colorsByHex.TryGetValue(hex, out color);
    }

    /// <summary>
    /// Gets a random color from the color list
    /// </summary>
    /// <returns>A NamedColor</returns>
    public NamedColor? GetRandomNamedColor()
    {
        if (namedColors.Length == 0)
            return null;

        Random random = new();
        int index = random.Next(namedColors.Length);
        return namedColors[index];
    }

    /// <summary>
    /// Searches for the closest node to the given LAB color values.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="l"></param>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="depth"></param>
    /// <param name="minDistance"></param>
    /// <param name="bestNode"></param>
    private static void SearchNearest(
        KDNode? node,
        float l,
        float a,
        float b,
        int depth,
        ref double minDistance,
        ref KDNode? bestNode)
    {
        if (node == null)
            return;

        int axis = depth % 3;
        (float, float, float) nodeLab = node.Color.Lab;

        float dim = axis switch
        {
            0 => l - nodeLab.Item1,
            1 => a - nodeLab.Item2,
            _ => b - nodeLab.Item3
        };

        double dist = Math.Sqrt(
            (l - nodeLab.Item1) * (l - nodeLab.Item1) +
            (a - nodeLab.Item2) * (a - nodeLab.Item2) +
            (b - nodeLab.Item3) * (b - nodeLab.Item3));

        if (dist < minDistance)
        {
            minDistance = dist;
            bestNode = node;
        }

        KDNode? firstChild = dim <= 0 ? node.Left : node.Right;
        KDNode? secondChild = dim <= 0 ? node.Right : node.Left;

        SearchNearest(firstChild, l, a, b, depth + 1, ref minDistance, ref bestNode);

        if (Math.Abs(dim) < minDistance)
            SearchNearest(secondChild, l, a, b, depth + 1, ref minDistance, ref bestNode);
    }

    /// <summary>
    /// Finds the colosest color for given RGB color values
    /// </summary>
    /// <param name="RGB">A touple with the RGB color values</param>
    /// <returns>A NamedColor</returns>
    public NamedColor? FindClosestColor((short, short, short) RGB)
    {
        (float l, float a, float b) = ColorConverter.RGBToLab(RGB);

        return FindClosestColor(l, a, b);
    }

    /// <summary>
    /// Finds the closest color for given RGB color values
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public NamedColor? FindClosestColor(short r, short g, short b)
    {
        return FindClosestColor((r, g, b));
    }

    /// <summary>
    /// Finds the closest color for given hex color value
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public NamedColor? FindClosestColor(string hex)
    {
        (short r, short g, short b) = ColorConverter.HexToRgb(hex);
        return FindClosestColor(r, g, b);
    }

    /// <summary>
    /// Finds the closest color for a given NamedColor
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public NamedColor? FindClosestColor(NamedColor color)
    {
        return FindClosestColor(color.Rgb);
    }

    /// <summary>
    /// Finds the closest color for given lab color values
    /// </summary>
    /// <param name="l"></param>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public NamedColor? FindClosestColor(float l, float a, float b)
    {
        double minDistance = double.PositiveInfinity;
        KDNode? bestNode = null;

        SearchNearest(ColorTreeRoot, l, a, b, 0, ref minDistance, ref bestNode);

        return bestNode?.Color;
    }

    /// <summary>
    /// Finds the closest color for given lab color values
    /// </summary>
    /// <param name="lab"></param>
    /// <returns></returns>
    public NamedColor? FindClosestColor((float, float, float) lab)
    {
        return FindClosestColor(lab.Item1, lab.Item2, lab.Item3);
    }

    /// <summary>
    /// Finds the closest color name for given RGB color values
    /// </summary>
    /// <param name="RGB"></param>
    /// <returns></returns>
    public string FindClosestColorName((short, short, short) RGB)
    {
        NamedColor? color = FindClosestColor(RGB);
        return color?.Name ?? "Unknown";
    }

    /// <summary>
    /// Finds the closest color name for given RGB color values
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public string FindClosestColorName(short r, short g, short b)
    {
        return FindClosestColorName((r, g, b));
    }

    /// <summary>
    /// Finds the closest color name for given hex color value
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public string FindClosestColorName(string hex)
    {
        NamedColor? color = FindClosestColor(hex);
        return color?.Name ?? "Unknown";
    }

    /// <summary>
    /// Finds the closest color name for a given NamedColor
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public string FindClosestColorName(NamedColor color)
    {
        return FindClosestColorName(color.Rgb);
    }

    /// <summary>
    /// Finds the closest color name for given lab color values
    /// </summary>
    /// <param name="l"></param>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public string FindClosestColorName(float l, float a, float b)
    {
        NamedColor? color = FindClosestColor(l, a, b);
        return color?.Name ?? "Unknown";
    }

    /// <summary>
    /// Finds the closest color name for given lab color values
    /// </summary>
    /// <param name="lab"></param>
    /// <returns></returns>
    public string FindClosestColorName((float, float, float) lab)
    {
        return FindClosestColorName(lab.Item1, lab.Item2, lab.Item3);
    }
}
