namespace AlgorithmVisualizer.Core.Algorithms.Sorting.Merge;

internal sealed class MergeSortElement
{
    public MergeSortElement(int value, int originalIndex)
    {
        Value = value;
        OriginalIndex = originalIndex;
    }

    public int Value { get; }
    public int OriginalIndex { get; }
    public MergeSortElementVisualState VisualState { get; set; }
}
