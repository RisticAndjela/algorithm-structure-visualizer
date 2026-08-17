namespace AlgorithmVisualizer.Core.Algorithms.Sorting.Selection;

public enum SelectionSortVariant
{
    Classic,
    StableShift
}

public enum SelectionSortElementVisualState
{
    Normal,
    Comparing,
    NewMinimum,
    SelectedMinimum,
    SwapRequired,
    Swapped,
    ShiftRequired,
    Shifted,
    Sorted
}

public enum SelectionSortPhase
{
    Ready,
    Selecting,
    Comparing,
    NewMinimum,
    SelectionComplete,
    Swapping,
    Shifting,
    PassComplete,
    Complete
}

public sealed record SelectionSortElementSnapshot(
    int Value,
    int OriginalIndex,
    SelectionSortElementVisualState VisualState);

public sealed record SelectionSortSnapshot(
    SelectionSortElementSnapshot[] Elements,
    int CurrentPass,
    int Comparisons,
    int Swaps,
    int Moves,
    int TargetIndex,
    int ScanIndex,
    int MinimumIndex,
    int SortedPrefixLength,
    SelectionSortPhase Phase,
    SelectionSortVariant Variant)
{
    public int Count => Elements.Length;
    public int RemainingCount => Math.Max(0, Count - SortedPrefixLength);
}

public sealed record SelectionSortResult(
    int[] InitialValues,
    int[] SortedValues,
    int Comparisons,
    int Swaps,
    int Moves,
    int Passes,
    bool PreservedEqualValueOrder,
    SelectionSortVariant Variant)
{
    public string BestCaseComplexity => "Θ(n²)";
    public string AverageCaseComplexity => "Θ(n²)";
    public string WorstCaseComplexity => "Θ(n²)";
    public string ExtraSpaceComplexity => "O(1)";
    public bool StableAlgorithm => Variant == SelectionSortVariant.StableShift;
    public bool RequiresRestartAfterMutation => true;
}
