namespace AlgorithmVisualizer.Core.DataStructures.Trees.Bst;

public enum BstOperationKind
{
    Insert,
    Search,
    Delete,
    Balance
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
/// Balance runs additionally report the Day-Stout-Warren rotation counts.
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
    BstDeleteCase DeleteCase,
    int VineRotations = 0,
    int CompressionRotations = 0,
    int CompressionPasses = 0)
{
    public int TotalChecks => Comparisons + SuccessorChecks;
    public int TotalRotations => VineRotations + CompressionRotations;
    public string WorstCaseComplexity => Operation == BstOperationKind.Balance ? "O(n)" : "O(h)";

    public string CurrentRunComplexity
    {
        get
        {
            if (Operation == BstOperationKind.Balance)
            {
                return InitialCount <= 1 ? "Θ(1)" : "Θ(n)";
            }

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
