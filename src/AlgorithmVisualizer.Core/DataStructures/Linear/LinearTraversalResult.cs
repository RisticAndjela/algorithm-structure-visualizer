namespace AlgorithmVisualizer.Core.DataStructures.Linear;

/// <summary>
/// Describes one completed linear lookup/delete simulation independently from the renderer.
/// Traversal work and physical storage mutation work are kept separate so the UI can teach
/// both the abstract algorithm and the concrete custom array-backed implementation truthfully.
/// </summary>
public sealed record LinearTraversalResult(
    string StructureName,
    LinearTraversalOperation Operation,
    LinearLookupCriterion Criterion,
    bool Found,
    Guid? ElementId,
    int? ElementValue,
    int Comparisons,
    int InitialCount,
    string TraversalDirection,
    string CurrentRunComplexity)
{
    /// <summary>Zero-based index in the custom backing-array order where the match was found.</summary>
    public int? MatchedIndex { get; init; }

    /// <summary>Number of references shifted left by the manual RemoveAt loop when deletion succeeds.</summary>
    public int ShiftedElements { get; init; }

    /// <summary>Custom backing-array capacity immediately before a successful keyed deletion.</summary>
    public int? CapacityBefore { get; init; }

    /// <summary>Custom backing-array capacity immediately after a successful keyed deletion.</summary>
    public int? CapacityAfter { get; init; }

    /// <summary>
    /// Concrete asymptotic class for the complete run in this implementation. For search this
    /// equals traversal complexity. For delete it includes both comparisons and array compaction.
    /// </summary>
    public string FullOperationComplexity { get; init; } = "Θ(1)";

    public int CountAfter => Operation == LinearTraversalOperation.Delete && Found
        ? Math.Max(InitialCount - 1, 0)
        : InitialCount;

    public int MajorWorkUnits => Comparisons + ShiftedElements;

    public string WorstCaseComplexity => "O(n)";
    public string BestCaseComplexity => "O(1)";
}

public enum LinearTraversalOperation
{
    Search,
    Delete
}

public enum LinearLookupCriterion
{
    Id,
    Value
}
