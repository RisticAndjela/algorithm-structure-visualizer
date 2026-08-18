namespace AlgorithmVisualizer.Core.Algorithms.Sorting.HeapSort;

public sealed class HeapSortElement
{
    public HeapSortElement(int value, int originalIndex)
    {
        Value = value;
        OriginalIndex = originalIndex;
    }

    public int Value { get; }
    public int OriginalIndex { get; }
}
