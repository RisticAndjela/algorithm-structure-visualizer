namespace AlgorithmVisualizer.Core.DataStructures.Heap;

public enum HeapKind
{
    Min,
    Max
}

public enum HeapOperationKind
{
    Insert,
    ExtractRoot,
    Search,
    Delete
}

public enum HeapRepairDirection
{
    None,
    BubbleUp,
    BubbleDown
}

/// <summary>
/// Renderer-neutral summary of one learner-triggered heap operation.
/// </summary>
public sealed record HeapOperationResult(
    HeapOperationKind Operation,
    HeapKind Kind,
    int? RequestedValue,
    bool Succeeded,
    Guid? AffectedElementId,
    int? AffectedValue,
    int Comparisons,
    int Swaps,
    int InitialCount,
    int FinalCount,
    int CapacityBefore,
    int CapacityAfter,
    int? StartIndex,
    int? EndIndex,
    HeapRepairDirection RepairDirection,
    string? RequestedDisplayId = null)
{
    public bool IsIdLookup => Operation == HeapOperationKind.Search && !string.IsNullOrWhiteSpace(RequestedDisplayId);

    public string? AffectedDisplayId => AffectedElementId.HasValue
        ? AffectedElementId.Value.ToString("N")[..6].ToUpperInvariant()
        : null;

    public string WorstCaseComplexity => Operation switch
    {
        HeapOperationKind.Search => "O(n)",
        HeapOperationKind.Delete => "O(n)",
        HeapOperationKind.Insert => "O(log n)",
        HeapOperationKind.ExtractRoot => "O(log n)",
        _ => "O(1)"
    };
}
