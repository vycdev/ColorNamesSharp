namespace ColorNamesSharp.Tree;

/// <summary>Builds a balanced three-dimensional KD-tree from named colors.</summary>
public class KDTreeBuilder
{
    /// <summary>Builds a KD-tree by recursively partitioning colors in CIELAB space.</summary>
    /// <param name="colors">The colors to include in the tree.</param>
    /// <param name="depth">The current recursion depth, normally zero for the root.</param>
    /// <returns>The root node, or <see langword="null"/> when <paramref name="colors"/> is empty.</returns>
    public static KDNode? Build(List<NamedColor> colors, int depth)
    {
        if (colors.Count == 0)
            return null;

        int axis = depth % 3;
        int mid = colors.Count / 2;

        colors.Sort((a, b) => axis switch
        {
            0 => a.Lab.Item1.CompareTo(b.Lab.Item1),
            1 => a.Lab.Item2.CompareTo(b.Lab.Item2),
            _ => a.Lab.Item3.CompareTo(b.Lab.Item3)
        });

        KDNode node = new(colors[mid])
        {
            Left = Build(colors.GetRange(0, mid), depth + 1),
            Right = Build(colors.GetRange(mid + 1, colors.Count - mid - 1), depth + 1)
        };

        return node;
    }
}
