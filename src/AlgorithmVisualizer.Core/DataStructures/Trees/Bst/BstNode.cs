namespace AlgorithmVisualizer.Core.DataStructures.Trees.Bst;

/// <summary>
/// One node in the manually implemented Binary Search Tree.
/// Child and parent references are maintained by <see cref="BstSimulation"/> rather than by a built-in tree collection.
/// </summary>
public sealed class BstNode
{
    internal BstNode(int value, BstNode? parent)
    {
        Id = Guid.NewGuid();
        Value = value;
        Parent = parent;
    }

    public Guid Id { get; }
    public string DisplayId => Id.ToString("N")[..6].ToUpperInvariant();
    public int Value { get; }
    public BstNodeVisualState VisualState { get; internal set; }

    internal BstNode? Parent { get; set; }
    internal BstNode? Left { get; set; }
    internal BstNode? Right { get; set; }
}

/// <summary>
/// Renderer-neutral transient state used by the BST learning UI.
/// </summary>
public enum BstNodeVisualState
{
    Normal,
    Checking,
    Visited,
    Matched,
    Adding,
    Removing,
    Replacement,
    PointerTarget
}
