namespace AlgorithmVisualizer.Core.DataStructures.Trees.Avl;

/// <summary>
/// Immutable AVL snapshot used by playback history and both visual/memory views.
/// </summary>
public sealed record AvlNodeSnapshot(
    Guid Id,
    int Value,
    int Height,
    int BalanceFactor,
    AvlNodeVisualState VisualState,
    string? ParentDisplayId,
    AvlNodeSnapshot? Left,
    AvlNodeSnapshot? Right)
{
    public string DisplayId => Id.ToString("N")[..6].ToUpperInvariant();

    internal static AvlNodeSnapshot? Capture(AvlNode? node)
    {
        if (node is null)
        {
            return null;
        }

        return new AvlNodeSnapshot(
            node.Id,
            node.Value,
            node.Height,
            GetHeight(node.Left) - GetHeight(node.Right),
            node.VisualState,
            node.Parent?.DisplayId,
            Capture(node.Left),
            Capture(node.Right));
    }

    private static int GetHeight(AvlNode? node) => node?.Height ?? 0;
}
