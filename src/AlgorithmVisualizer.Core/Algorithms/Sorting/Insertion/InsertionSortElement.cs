namespace AlgorithmVisualizer.Core.Algorithms.Sorting.Insertion;

internal sealed class InsertionSortElement
{
    public InsertionSortElement(int value, int originalIndex)
    {
        Value = value;
        OriginalIndex = originalIndex;
    }

    public int Value { get; }
    public int OriginalIndex { get; }
    public InsertionSortElementVisualState VisualState { get; set; }
}
