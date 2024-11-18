namespace ColorNames.Tree;

public class KDNode(NamedColor color)
{
    public KDNode? Left { get; set; }
    public KDNode? Right { get; set; }
    public NamedColor Color { get; set; } = color;
}
