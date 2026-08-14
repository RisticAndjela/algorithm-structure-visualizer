namespace AlgorithmVisualizer.Core.DataStructures.Trees.RedBlack;

public enum RedBlackColor
{
    Red,
    Black
}

/// <summary>
/// One node in the manually implemented Red-Black Tree.
/// Parent/child references and color are maintained by <see cref="RedBlackSimulation"/>.
/// Null child references represent the conceptual black NIL leaves used by the algorithm.
/// </summary>
public sealed class RedBlackNode
{
    internal RedBlackNode(int value, RedBlackNode? parent)
    {
        Id = Guid.NewGuid();
        Value = value;
        Parent = parent;
        Color = RedBlackColor.Red;
    }

    public Guid Id { get; }
    public string DisplayId => Id.ToString("N")[..6].ToUpperInvariant();
    public int Value { get; }
    public RedBlackColor Color { get; internal set; }
    public RedBlackNodeVisualState VisualState { get; internal set; }

    internal RedBlackNode? Parent { get; set; }
    internal RedBlackNode? Left { get; set; }
    internal RedBlackNode? Right { get; set; }
}

/// <summary>
/// Renderer-neutral transient state for the Red-Black learning UI.
/// Node color is a separate algorithmic property; these states explain the current step.
/// </summary>
public enum RedBlackNodeVisualState
{
    Normal,
    Checking,
    Visited,
    Matched,
    Adding,
    Removing,
    Replacement,
    PointerTarget,
    Violation,
    RelativeFocus,
    RotationPivot,
    Rotating,
    Recolored,
    Fixed
}
