namespace AlgorithmVisualizer.Core.DataStructures.Heap;

public sealed record HeapElementSnapshot(
    int Index,
    Guid Id,
    int Value,
    HeapElementVisualState VisualState)
{
    public string DisplayId => Id.ToString("N")[..6].ToUpperInvariant();
    public int? ParentIndex => Index == 0 ? null : (Index - 1) / 2;
    public int LeftChildIndex => (2 * Index) + 1;
    public int RightChildIndex => (2 * Index) + 2;
}

public sealed record HeapSnapshot(
    HeapKind Kind,
    int Count,
    int Capacity,
    HeapElementSnapshot[] Elements)
{
    public HeapElementSnapshot? Root => Count == 0 ? null : Elements[0];
}
