namespace AlgorithmVisualizer.Core.DataStructures.Trees.Bst;

public enum BstOperationKind
{
    Insert,
    Search,
    Delete
}

public enum BstDeleteCase
{
    None,
    Leaf,
    OneChild,
    TwoChildren
}

/// <summary>
/// Summarizes one learner-triggered BST operation without coupling Core logic to the UI.
/// </summary>
public sealed record BstOperationResult(
    BstOperationKind Operation,
    int RequestedValue,
    bool Succeeded,
    bool DuplicateRejected,
    Guid? AffectedNodeId,
    int Comparisons,
    int SuccessorChecks,
    int InitialCount,
    int FinalCount,
    int HeightBefore,
    int HeightAfter,
    BstDeleteCase DeleteCase)
{
    public int TotalChecks => Comparisons + SuccessorChecks;
    public string WorstCaseComplexity => "O(h)";

    public string CurrentRunComplexity
    {
        get
        {
            if (TotalChecks <= 1)
            {
                return "Θ(1)";
            }

            var relevantHeight = Math.Max(HeightBefore, HeightAfter);
            return TotalChecks >= Math.Max(2, relevantHeight) ? "Θ(h)" : "Θ(k)";
        }
    }

    public string? AffectedDisplayId => AffectedNodeId.HasValue
        ? AffectedNodeId.Value.ToString("N")[..6].ToUpperInvariant()
        : null;
}
