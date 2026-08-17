namespace AlgorithmVisualizer.Core.Algorithms.Sorting.Insertion;

public enum InsertionSortVariant
{
    Linear,
    BinarySearch
}

public enum InsertionSortElementVisualState
{
    Normal,
    Sorted,
    Comparing,
    ShiftRequired,
    Shifted,
    Inserted
}

public enum InsertionSortPhase
{
    Ready,
    SelectKey,
    Searching,
    Shifting,
    Inserting,
    PassComplete,
    Complete
}

public sealed record InsertionSortElementSnapshot(
    bool Occupied,
    int Value,
    int OriginalIndex,
    InsertionSortElementVisualState VisualState);

public sealed record InsertionSortSnapshot(
    InsertionSortElementSnapshot[] Elements,
    int CurrentPass,
    int Comparisons,
    int Shifts,
    int Writes,
    int SortedPrefixLength,
    int KeySourceIndex,
    int? HeldKeyValue,
    int? HeldKeyOriginalIndex,
    int CompareIndex,
    int InsertionIndex,
    int GapIndex,
    int SearchLow,
    int SearchHigh,
    int SearchMid,
    InsertionSortPhase Phase,
    InsertionSortVariant Variant)
{
    public int Count => Elements.Length;
}

public sealed record InsertionSortResult(
    int[] InitialValues,
    int[] SortedValues,
    int Comparisons,
    int Shifts,
    int Writes,
    int Passes,
    bool PreservedEqualValueOrder,
    InsertionSortVariant Variant)
{
    public string BestCaseComplexity => Variant == InsertionSortVariant.Linear ? "Θ(n)" : "Θ(n log n)";
    public string AverageCaseComplexity => "Θ(n²)";
    public string WorstCaseComplexity => "Θ(n²)";
    public string ExtraSpaceComplexity => "O(1)";
    public bool StableAlgorithm => true;
    public bool SupportsOnlineInsertion => true;
    public bool DeletePreservesSortedOrder => true;
    public bool UpdateCanBeRepairedByReinsert => true;
}
