namespace AlgorithmVisualizer.Core.Algorithms.Sorting.Selection;

internal sealed class SelectionSortElement
{
    public SelectionSortElement(int value, int originalIndex)
    {
        Value = value;
        OriginalIndex = originalIndex;
    }

    public int Value { get; }
    public int OriginalIndex { get; }
    public SelectionSortElementVisualState VisualState { get; set; }
}
