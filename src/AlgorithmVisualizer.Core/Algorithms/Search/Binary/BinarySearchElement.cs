namespace AlgorithmVisualizer.Core.Algorithms.Search.Binary;

internal sealed class BinarySearchElement
{
    public BinarySearchElement(int value, int originalIndex)
    {
        Value = value;
        OriginalIndex = originalIndex;
    }

    public int Value { get; }
    public int OriginalIndex { get; }
    public BinarySearchElementVisualState VisualState { get; set; }
}
