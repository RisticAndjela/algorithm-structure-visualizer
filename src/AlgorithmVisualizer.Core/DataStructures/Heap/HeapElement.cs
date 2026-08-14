namespace AlgorithmVisualizer.Core.DataStructures.Heap;

/// <summary>
/// One logical value stored by the teaching heap. Identity is stable while the element moves
/// between array indexes during bubble-up / bubble-down swaps.
/// </summary>
public sealed class HeapElement
{
    internal HeapElement(int value)
    {
        Id = Guid.NewGuid();
        Value = value;
    }

    public Guid Id { get; }
    public string DisplayId => Id.ToString("N")[..6].ToUpperInvariant();
    public int Value { get; }
    public HeapElementVisualState VisualState { get; internal set; }
}

public enum HeapElementVisualState
{
    Normal,
    Checking,
    Candidate,
    Swapping,
    Added,
    Removing,
    Matched,
    Repairing
}
