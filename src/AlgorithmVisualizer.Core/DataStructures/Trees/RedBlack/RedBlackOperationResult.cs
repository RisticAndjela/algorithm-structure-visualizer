namespace AlgorithmVisualizer.Core.DataStructures.Trees.RedBlack;

public enum RedBlackOperationKind
{
    Insert,
    Search,
    Delete
}

public enum RedBlackDeleteCase
{
    None,
    Leaf,
    OneChild,
    TwoChildren
}

/// <summary>
/// First balancing situation encountered in a run. Insert and delete use different fix-up cases.
/// </summary>
public enum RedBlackRepairCase
{
    None,
    InsertRootBlack,
    InsertUncleRed,
    InsertTriangle,
    InsertLine,
    DeleteSiblingRed,
    DeleteSiblingBlackChildrenBlack,
    DeleteNearRed,
    DeleteFarRed
}

/// <summary>
/// Summary of one learner-triggered Red-Black Tree operation.
/// </summary>
public sealed record RedBlackOperationResult(
    RedBlackOperationKind Operation,
    int RequestedValue,
    bool Succeeded,
    bool DuplicateRejected,
    Guid? AffectedNodeId,
    int Comparisons,
    int SuccessorChecks,
    int FixupChecks,
    int RecolorCount,
    int RotationCount,
    RedBlackRepairCase FirstRepairCase,
    int InitialCount,
    int FinalCount,
    int HeightBefore,
    int HeightAfter,
    int BlackHeightBefore,
    int BlackHeightAfter,
    RedBlackDeleteCase DeleteCase,
    string? RequestedDisplayId = null)
{
    public int TotalChecks => Comparisons + SuccessorChecks + FixupChecks;
    public bool IsIdLookup => Operation == RedBlackOperationKind.Search && !string.IsNullOrWhiteSpace(RequestedDisplayId);
    public string WorstCaseComplexity => IsIdLookup ? "O(n)" : "O(log n)";
    public string CurrentRunComplexity => TotalChecks <= 1 && RotationCount == 0 ? "Θ(1)" : "Θ(k)";

    public string? AffectedDisplayId => AffectedNodeId.HasValue
        ? AffectedNodeId.Value.ToString("N")[..6].ToUpperInvariant()
        : null;
}
