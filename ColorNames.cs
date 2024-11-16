using color_names_csharp.tree;
using color_names_csharp.Utility;
using System.ComponentModel.Design.Serialization;

namespace color_names_csharp;
public class ColorNames(List<NamedColor> namedColors)
{
    public KDNode? ColorTreeRoot { get; } = KDTreeBuilder.Build(namedColors, 0);
    private double MinDistance = double.PositiveInfinity;
    private KDNode? BestNode = null;

    /// <summary>
    /// Gets a random color from the color list
    /// </summary>
    /// <returns>A NamedColor</returns>
    public NamedColor? GetRandomNamedColor()
    {
        if (namedColors.Count == 0)
            return null;

        Random random = new();
        int index = random.Next(namedColors.Count);
        return namedColors[index];
    }

    /// <summary>
    /// Sets the BestNode to the closest possible match of the given lab color values. 
    /// </summary>
    /// <param name="node"></param>
    /// <param name="l"></param>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="depth"></param>
    private void SearchNearest(KDNode? node, float l, float a, float b, int depth)
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
            ((l - nodeLab.Item1) * (l - nodeLab.Item1)) +
            ((a - nodeLab.Item2) * (a - nodeLab.Item2)) +
            ((b - nodeLab.Item3) * (b - nodeLab.Item3)));

        if (dist < MinDistance)
        {
            MinDistance = dist;
            BestNode = node;
        }

        KDNode? firstChild = (dim <= 0) ? node.Left : node.Right;
        KDNode? secondChild = (dim <= 0) ? node.Right : node.Left;

        SearchNearest(firstChild, l, a, b, depth + 1);

        if (dim * dim < MinDistance)
            SearchNearest(secondChild, l, a, b, depth + 1);
    }

    /// <summary>
    /// Finds the colosest color for given RGB color values
    /// </summary>
    /// <param name="RGB">A touple with the RGB color values</param>
    /// <returns>A NamedColor</returns>
    public NamedColor? FindClosestColor((short, short, short) RGB)
    {
        MinDistance = double.PositiveInfinity;
        BestNode = null; 

        (float l, float a, float b) = ColorConverter.RGBToLab(RGB);

        SearchNearest(ColorTreeRoot, l, a, b, 0);

        return BestNode?.Color;
    }
}
