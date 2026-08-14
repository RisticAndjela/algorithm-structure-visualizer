namespace AlgorithmVisualizer.Core.DataStructures.Trees.Avl;

public enum AvlOperationKind
{
    Insert,
    Search,
    Delete
}

public enum AvlDeleteCase
{
    None,
    Leaf,
    OneChild,
    TwoChildren
}

/// <summary>
/// The imbalance pattern that selected an AVL repair.
/// LR/RL use two primitive rotations but represent one diagnosed imbalance case.
/// </summary>
public enum AvlRotationCase
{
    None,
    LL,
    RR,
    LR,
    RL
}

/// <summary>
/// Summarizes one learner-triggered AVL operation without coupling Core logic to Blazor.
/// </summary>
public sealed record AvlOperationResult(
    AvlOperationKind Operation,
    int RequestedValue,
    bool Succeeded,
    bool DuplicateRejected,
    Guid? AffectedNodeId,
    int Comparisons,
    int SuccessorChecks,
    int RebalanceChecks,
    int RotationCount,
    AvlRotationCase FirstRotationCase,
    int InitialCount,
    int FinalCount,
    int HeightBefore,
    int HeightAfter,
    AvlDeleteCase DeleteCase)
{
    public int TotalChecks => Comparisons + SuccessorChecks + RebalanceChecks;
    public string WorstCaseComplexity => "O(log n)";
    public string CurrentRunComplexity => TotalChecks <= 1 && RotationCount == 0 ? "Θ(1)" : "Θ(k)";

    public string? AffectedDisplayId => AffectedNodeId.HasValue
        ? AffectedNodeId.Value.ToString("N")[..6].ToUpperInvariant()
        : null;
}
