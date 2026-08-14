namespace AlgorithmVisualizer.Core.DataStructures.Trees.Bst;

/// <summary>
/// Immutable tree snapshot used for display-history playback. It preserves node identity and links
/// without exposing mutable algorithm state to the Blazor presentation layer.
/// </summary>
public sealed record BstNodeSnapshot(
    Guid Id,
    int Value,
    BstNodeVisualState VisualState,
    string? ParentDisplayId,
    BstNodeSnapshot? Left,
    BstNodeSnapshot? Right)
{
    public string DisplayId => Id.ToString("N")[..6].ToUpperInvariant();

    internal static BstNodeSnapshot? Capture(BstNode? node)
    {
        if (node is null)
        {
            return null;
        }

        return new BstNodeSnapshot(
            node.Id,
            node.Value,
            node.VisualState,
            node.Parent?.DisplayId,
            Capture(node.Left),
            Capture(node.Right));
    }
}
