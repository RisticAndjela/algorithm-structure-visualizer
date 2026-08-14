namespace AlgorithmVisualizer.Core.DataStructures.Trees.RedBlack;

/// <summary>
/// Immutable Red-Black Tree snapshot used by playback history and visual/memory views.
/// </summary>
public sealed record RedBlackNodeSnapshot(
    Guid Id,
    int Value,
    RedBlackColor Color,
    RedBlackNodeVisualState VisualState,
    string? ParentDisplayId,
    RedBlackNodeSnapshot? Left,
    RedBlackNodeSnapshot? Right)
{
    public string DisplayId => Id.ToString("N")[..6].ToUpperInvariant();

    internal static RedBlackNodeSnapshot? Capture(RedBlackNode? node)
    {
        if (node is null)
        {
            return null;
        }

        return new RedBlackNodeSnapshot(
            node.Id,
            node.Value,
            node.Color,
            node.VisualState,
            node.Parent?.DisplayId,
            Capture(node.Left),
            Capture(node.Right));
    }
}
