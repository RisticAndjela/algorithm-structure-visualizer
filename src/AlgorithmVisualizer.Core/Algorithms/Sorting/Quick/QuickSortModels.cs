namespace AlgorithmVisualizer.Core.Algorithms.Sorting.Quick;

public enum QuickSortVariant
{
    LomutoLastPivot,
    MedianOfThreeThreeWay
}

public enum QuickSortPhase
{
    Ready,
    ChoosePivot,
    Scanning,
    Swapping,
    PlacePivot,
    PartitionComplete,
    BaseCase,
    Complete
}

public enum QuickSortElementVisualState
{
    Normal,
    ActiveRange,
    Pivot,
    Scan,
    Boundary,
    LessRegion,
    EqualRegion,
    GreaterRegion,
    Swapping,
    Sorted
}

public sealed record QuickSortElementSnapshot(
    int Value,
    int OriginalIndex,
    QuickSortElementVisualState VisualState);

public sealed record QuickSortSnapshot(
    int[] InitialValues,
    QuickSortElementSnapshot[] Elements,
    bool[] Finalized,
    int Comparisons,
    int Swaps,
    int Partitions,
    int CurrentDepth,
    int MaxDepth,
    int ActiveStart,
    int ActiveEnd,
    int PivotIndex,
    int? PivotValue,
    int ScanIndex,
    int BoundaryIndex,
    int LessEnd,
    int EqualStart,
    int EqualEnd,
    int GreaterStart,
    QuickSortPhase Phase,
    QuickSortVariant Variant)
{
    public int Count => Elements.Length;
}

public sealed record QuickSortResult(
    int[] InitialValues,
    int[] SortedValues,
    int Comparisons,
    int Swaps,
    int Partitions,
    int MaxDepth,
    bool PreservedEqualValueOrder,
    QuickSortVariant Variant)
{
    public string BestCaseComplexity => Variant == QuickSortVariant.MedianOfThreeThreeWay ? "Θ(n) when all values equal" : "Θ(n log n)";
    public string AverageCaseComplexity => "Θ(n log n)";
    public string WorstCaseComplexity => "Θ(n²)";
    public string ExtraArraySpaceComplexity => "O(1)";
    public string AverageRecursionSpaceComplexity => "O(log n)";
    public string WorstRecursionSpaceComplexity => "O(n)";
    public bool StableAlgorithm => false;
    public bool ActiveMutationRequiresRestart => true;
    public bool HandlesDuplicateHeavyInputBetter => Variant == QuickSortVariant.MedianOfThreeThreeWay;
}
