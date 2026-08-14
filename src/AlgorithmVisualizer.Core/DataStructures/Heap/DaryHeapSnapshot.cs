namespace AlgorithmVisualizer.Core.DataStructures.Heap;

/// <summary>
/// Renderer-neutral snapshot for the generalized d-ary heap lab.
/// A binary heap is the special d = 2 case; this lab defaults to d = 3 so the distinction is visible.
/// </summary>
public sealed record DaryHeapElementSnapshot(
    int Index,
    Guid Id,
    int Value,
    HeapElementVisualState VisualState)
{
    public string DisplayId => Id.ToString("N")[..6].ToUpperInvariant();

    public int? ParentIndex(int arity) => Index == 0 ? null : (Index - 1) / arity;

    public int FirstChildIndex(int arity) => (arity * Index) + 1;

    public int LastPossibleChildIndex(int arity) => (arity * Index) + arity;
}

public sealed record DaryHeapSnapshot(
    HeapKind Kind,
    int Arity,
    int Count,
    int Capacity,
    DaryHeapElementSnapshot[] Elements)
{
    public DaryHeapElementSnapshot? Root => Count == 0 ? null : Elements[0];
}
