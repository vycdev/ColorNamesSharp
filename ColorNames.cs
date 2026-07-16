using ColorNamesSharp.Tree;
using ColorNamesSharp.Utility;

namespace ColorNamesSharp;

/// <summary>
/// Finds exact or perceptually nearest matches in a configured collection of named colors.
/// </summary>
public class ColorNames
{
    private readonly NamedColor[] namedColors;
    private readonly Dictionary<string, NamedColor> colorsByName;
    private readonly Dictionary<string, NamedColor> colorsByHex;

    /// <summary>
    /// The configured colors in their original insertion order.
    /// </summary>
    public IReadOnlyList<NamedColor> Colors { get; }

    /// <summary>Gets the root of the CIELAB KD-tree used for nearest-neighbor searches.</summary>
    public KDNode? ColorTreeRoot { get; }

    /// <summary>Creates a lookup over the supplied colors.</summary>
    /// <param name="namedColors">The colors to index, in insertion order.</param>
    /// <exception cref="ArgumentNullException"><paramref name="namedColors"/> is <see langword="null"/>.</exception>
    public ColorNames(IEnumerable<NamedColor> namedColors)
    {
        if (namedColors == null)
            throw new ArgumentNullException(nameof(namedColors));

        this.namedColors = namedColors.ToArray();
        Colors = Array.AsReadOnly(this.namedColors);
        ColorTreeRoot = KDTreeBuilder.Build(this.namedColors.ToList(), 0);
        colorsByName = new Dictionary<string, NamedColor>(StringComparer.OrdinalIgnoreCase);
        colorsByHex = new Dictionary<string, NamedColor>(StringComparer.OrdinalIgnoreCase);

        foreach (NamedColor color in this.namedColors)
        {
            if (!colorsByName.ContainsKey(color.Name))
                colorsByName.Add(color.Name, color);

            if (!colorsByHex.ContainsKey(color.Hex))
                colorsByHex.Add(color.Hex, color);
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
    /// Gets a random color from the configured color list.
    /// </summary>
    /// <returns>A random color, or <see langword="null"/> when the list is empty.</returns>
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
    /// Finds the closest color for the given RGB color values.
    /// </summary>
    /// <param name="RGB">The red, green, and blue channel values.</param>
    /// <returns>The closest configured color, or <see langword="null"/> when the list is empty.</returns>
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
