namespace AlgorithmVisualizer.Core.DataStructures.Trees.Avl;

/// <summary>
/// One node in the manually implemented AVL tree.
/// Parent/child references and cached height are maintained by <see cref="AvlSimulation"/>.
/// </summary>
public sealed class AvlNode
{
    internal AvlNode(int value, AvlNode? parent)
    {
        Id = Guid.NewGuid();
        Value = value;
        Parent = parent;
        Height = 1;
    }

    public Guid Id { get; }
    public string DisplayId => Id.ToString("N")[..6].ToUpperInvariant();
    public int Value { get; }
    public int Height { get; internal set; }
    public AvlNodeVisualState VisualState { get; internal set; }

    internal AvlNode? Parent { get; set; }
    internal AvlNode? Left { get; set; }
    internal AvlNode? Right { get; set; }
}

/// <summary>
/// Renderer-neutral transient state used by the AVL learning UI.
/// </summary>
public enum AvlNodeVisualState
{
    Normal,
    Checking,
    Visited,
    Matched,
    Adding,
    Removing,
    Replacement,
    PointerTarget,
    Unbalanced,
    RotationPivot,
    Rotating,
    Balanced
}
