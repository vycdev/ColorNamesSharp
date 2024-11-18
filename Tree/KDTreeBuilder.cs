namespace ColorNames.Tree;

public class KDTreeBuilder
{
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

        KDNode node = new(colors[mid]);

        node.Left = Build(colors[0..mid], depth + 1);
        node.Right = Build(colors[(mid + 1)..colors.Count], depth + 1);

        return node;
    }
}
