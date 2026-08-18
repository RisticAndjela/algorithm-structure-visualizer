namespace AlgorithmVisualizer.Core.Algorithms.Search.Linear;

public enum LinearSearchElementVisualState
{
    Unvisited,
    Current,
    Checked,
    Found
}

public enum LinearSearchPhase
{
    Ready,
    Checking,
    Found,
    NotFound,
    Complete
}

public sealed record LinearSearchElementSnapshot(
    int Value,
    int OriginalIndex,
    LinearSearchElementVisualState VisualState);

public sealed record LinearSearchSnapshot(
    LinearSearchElementSnapshot[] Elements,
    int Target,
    int CurrentIndex,
    int Comparisons,
    int CheckedCount,
    int? FoundIndex,
    LinearSearchPhase Phase)
{
    public int Count => Elements.Length;
    public int RemainingCount => Math.Max(0, Count - CheckedCount);
}

public sealed record LinearSearchResult(
    int[] InitialValues,
    int Target,
    bool Found,
    int? FoundIndex,
    int Comparisons,
    int CheckedCount,
    int? FirstOccurrenceIndex)
{
    public string BestCaseComplexity => "Θ(1)";
    public string AverageCaseComplexity => "Θ(n)";
    public string WorstCaseComplexity => "Θ(n)";
    public string ExtraSpaceComplexity => "O(1)";
    public bool ReturnsFirstOccurrence => Found && FoundIndex == FirstOccurrenceIndex;
    public bool RequiresRestartAfterMutation => true;
}
