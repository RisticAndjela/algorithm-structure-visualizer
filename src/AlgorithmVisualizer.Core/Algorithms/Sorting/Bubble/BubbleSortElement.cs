namespace AlgorithmVisualizer.Core.Algorithms.Sorting.Bubble;

internal sealed class BubbleSortElement
{
    public BubbleSortElement(int value, int originalIndex)
    {
        Value = value;
        OriginalIndex = originalIndex;
    }

    public int Value { get; }
    public int OriginalIndex { get; }
    public BubbleSortElementVisualState VisualState { get; set; }
}
