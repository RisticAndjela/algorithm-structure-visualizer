namespace AlgorithmVisualizer.Core.Algorithms.Search.Linear;

internal sealed class LinearSearchElement
{
    public LinearSearchElement(int value, int originalIndex)
    {
        Value = value;
        OriginalIndex = originalIndex;
    }

    public int Value { get; }
    public int OriginalIndex { get; }
    public LinearSearchElementVisualState VisualState { get; set; }
}
