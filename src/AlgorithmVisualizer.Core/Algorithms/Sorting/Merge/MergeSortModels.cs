namespace AlgorithmVisualizer.Core.Algorithms.Sorting.Merge;

public enum MergeSortVariant
{
    TopDownRecursive,
    NaturalRuns
}

public enum MergeSortElementVisualState
{
    Normal,
    ActiveRange,
    LeftRun,
    RightRun,
    Comparing,
    Chosen,
    Writing,
    Sorted
}

public enum MergeSortPhase
{
    Ready,
    Splitting,
    DetectingRuns,
    Comparing,
    FillingBuffer,
    CopyingBack,
    MergeComplete,
    Complete
}

public sealed record MergeSortElementSnapshot(
    int Value,
    int OriginalIndex,
    MergeSortElementVisualState VisualState);

public sealed record MergeSortBufferSlotSnapshot(
    bool Occupied,
    int Value,
    int OriginalIndex,
    bool IsWriteTarget);

public sealed record MergeSortSnapshot(
    MergeSortElementSnapshot[] Elements,
    MergeSortBufferSlotSnapshot[] Buffer,
    int Comparisons,
    int Writes,
    int Merges,
    int Splits,
    int CurrentDepth,
    int MaxDepth,
    int ActiveStart,
    int LeftEnd,
    int RightStart,
    int ActiveEnd,
    int LeftReadIndex,
    int RightReadIndex,
    int WriteIndex,
    int NaturalRunCount,
    int NaturalPass,
    MergeSortPhase Phase,
    MergeSortVariant Variant)
{
    public int Count => Elements.Length;
}

public sealed record MergeSortResult(
    int[] InitialValues,
    int[] SortedValues,
    int Comparisons,
    int Writes,
    int Merges,
    int Splits,
    int MaxDepth,
    int InitialNaturalRunCount,
    int NaturalPasses,
    bool PreservedEqualValueOrder,
    MergeSortVariant Variant)
{
    public string BestCaseComplexity => Variant == MergeSortVariant.NaturalRuns ? "Θ(n) on one natural run" : "Θ(n log n)";
    public string AverageCaseComplexity => "Θ(n log n)";
    public string WorstCaseComplexity => "Θ(n log n)";
    public string ExtraSpaceComplexity => "O(n)";
    public bool StableAlgorithm => true;
    public bool ActiveMutationRequiresRestart => true;
}
