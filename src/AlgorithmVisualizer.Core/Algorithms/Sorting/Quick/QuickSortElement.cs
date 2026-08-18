namespace AlgorithmVisualizer.Core.Algorithms.Sorting.Quick;

public sealed class QuickSortElement
{
    public QuickSortElement(int value, int originalIndex)
    {
        Value = value;
        OriginalIndex = originalIndex;
    }

    public int Value { get; }
    public int OriginalIndex { get; }
}
