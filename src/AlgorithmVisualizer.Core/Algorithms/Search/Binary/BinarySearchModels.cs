namespace AlgorithmVisualizer.Core.Algorithms.Search.Binary;

public enum BinarySearchVariant
{
    AnyMatch,
    FirstOccurrence
}

public enum BinarySearchElementVisualState
{
    Active,
    Current,
    Eliminated,
    Candidate,
    Found
}

public enum BinarySearchPhase
{
    Ready,
    Checking,
    CandidateFound,
    Found,
    NotFound,
    Complete
}

public sealed record BinarySearchElementSnapshot(
    int Value,
    int OriginalIndex,
    BinarySearchElementVisualState VisualState);

public sealed record BinarySearchSnapshot(
    BinarySearchElementSnapshot[] Elements,
    int Target,
    BinarySearchVariant Variant,
    int Left,
    int Right,
    int Mid,
    int Comparisons,
    int RangeReductions,
    int? CandidateIndex,
    int? FoundIndex,
    BinarySearchPhase Phase)
{
    public int Count => Elements.Length;
    public int ActiveCount => Left <= Right ? Right - Left + 1 : 0;
}

public sealed record BinarySearchResult(
    int[] InitialValues,
    int Target,
    BinarySearchVariant Variant,
    bool Found,
    int? FoundIndex,
    int Comparisons,
    int RangeReductions,
    int? FirstOccurrenceIndex)
{
    public string BestCaseComplexity => "Θ(1)";
    public string AverageCaseComplexity => "Θ(log n)";
    public string WorstCaseComplexity => "Θ(log n)";
    public string ExtraSpaceComplexity => "O(1)";
    public bool RequiresSortedInput => true;
    public bool RequiresRestartAfterMutation => true;
    public bool ReturnsFirstOccurrence => Found && FoundIndex == FirstOccurrenceIndex;
}
