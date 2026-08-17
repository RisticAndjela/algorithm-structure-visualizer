namespace AlgorithmVisualizer.Core.Algorithms.Sorting.Bubble;

public enum BubbleSortVariant
{
    Basic,
    Optimized
}

public enum BubbleSortElementVisualState
{
    Normal,
    Comparing,
    SwapRequired,
    Kept,
    Swapped,
    Sorted
}

public enum BubbleSortPhase
{
    Ready,
    Comparing,
    Deciding,
    Swapping,
    PassComplete,
    Complete
}

public sealed record BubbleSortElementSnapshot(
    int Value,
    int OriginalIndex,
    BubbleSortElementVisualState VisualState);

public sealed record BubbleSortSnapshot(
    BubbleSortElementSnapshot[] Elements,
    int CurrentPass,
    int Comparisons,
    int Swaps,
    int PassSwaps,
    int ActiveLeftIndex,
    int ActiveRightIndex,
    int SortedSuffixStart,
    bool EarlyExit,
    BubbleSortPhase Phase,
    BubbleSortVariant Variant)
{
    public int Count => Elements.Length;
    public int SortedCount => SortedSuffixStart >= Count ? 0 : Count - SortedSuffixStart;
}

public sealed record BubbleSortResult(
    int[] InitialValues,
    int[] SortedValues,
    int Comparisons,
    int Swaps,
    int Passes,
    bool UsedEarlyExit,
    bool Stable,
    BubbleSortVariant Variant)
{
    public string BestCaseComplexity => Variant == BubbleSortVariant.Optimized ? "Θ(n)" : "Θ(n²)";
    public string AverageCaseComplexity => "Θ(n²)";
    public string WorstCaseComplexity => "Θ(n²)";
    public string ExtraSpaceComplexity => "O(1)";
    public bool RequiresRestartAfterMutation => true;
}
