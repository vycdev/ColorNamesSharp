namespace ColorNamesSharp.Tree;

/// <summary>Represents a node in the three-dimensional CIELAB KD-tree.</summary>
/// <param name="color">The color stored at this node.</param>
public class KDNode(NamedColor color)
{
    /// <summary>Gets or sets the child containing lower values on the current axis.</summary>
    public KDNode? Left { get; set; }
    /// <summary>Gets or sets the child containing higher values on the current axis.</summary>
    public KDNode? Right { get; set; }
    /// <summary>Gets or sets the color stored at this node.</summary>
    public NamedColor Color { get; set; } = color;
}
