namespace AlgorithmVisualizer.Core.Algorithms.Sorting.HeapSort;

public enum HeapSortVariant
{
    IncrementalBuild,
    FloydBottomUp
}

public enum HeapSortPhase
{
    Ready,
    BuildInsert,
    BuildCompare,
    BuildSwap,
    BuildHeapify,
    BuildComplete,
    ExtractRoot,
    ShrinkHeap,
    RepairCompare,
    RepairSwap,
    RepairComplete,
    Complete
}

public enum HeapSortElementVisualState
{
    Normal,
    Unbuilt,
    ActiveHeap,
    Root,
    BuildItem,
    Parent,
    ChildCandidate,
    Comparing,
    Swapping,
    SortedSuffix
}

public sealed record HeapSortElementSnapshot(
    int Value,
    int OriginalIndex,
    HeapSortElementVisualState VisualState);

public sealed record HeapSortSnapshot(
    int[] InitialValues,
    HeapSortElementSnapshot[] Elements,
    int HeapSize,
    int Comparisons,
    int Swaps,
    int BuildComparisons,
    int BuildSwaps,
    int Extractions,
    int SiftDownCalls,
    int BuildIndex,
    int ParentIndex,
    int LeftChildIndex,
    int RightChildIndex,
    int CandidateIndex,
    int SwapLeftIndex,
    int SwapRightIndex,
    bool BuildFinished,
    HeapSortPhase Phase,
    HeapSortVariant Variant)
{
    public int Count => Elements.Length;
    public int SortedSuffixStart => BuildFinished ? HeapSize : Elements.Length;
}

public sealed record HeapSortResult(
    int[] InitialValues,
    int[] SortedValues,
    int Comparisons,
    int Swaps,
    int BuildComparisons,
    int BuildSwaps,
    int Extractions,
    int SiftDownCalls,
    bool PreservedEqualValueOrder,
    HeapSortVariant Variant)
{
    public string BestCaseComplexity => "Θ(n log n)";
    public string AverageCaseComplexity => "Θ(n log n)";
    public string WorstCaseComplexity => "Θ(n log n)";
    public string BuildComplexity => Variant == HeapSortVariant.FloydBottomUp ? "Θ(n)" : "O(n log n)";
    public string ExtraArraySpaceComplexity => "O(1)";
    public bool StableAlgorithm => false;
    public bool ActiveMutationRequiresRestart => true;
    public bool UsesLinearHeapConstruction => Variant == HeapSortVariant.FloydBottomUp;
}
