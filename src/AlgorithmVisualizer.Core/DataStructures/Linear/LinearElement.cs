namespace AlgorithmVisualizer.Core.DataStructures.Linear;

/// <summary>
/// Represents one value in a linear data structure together with renderer-neutral
/// transient state used by visual simulations.
/// </summary>
public sealed class LinearElement
{
    public LinearElement(int value, LinearElementVisualState visualState = LinearElementVisualState.Normal)
    {
        Id = Guid.NewGuid();
        Value = value;
        VisualState = visualState;
    }

    public Guid Id { get; }
    public int Value { get; }
    public LinearElementVisualState VisualState { get; internal set; }
}

/// <summary>
/// Describes the semantic visual state of an element without coupling Core code to CSS or a renderer.
/// </summary>
public enum LinearElementVisualState
{
    Normal,
    Adding,
    Removing,
    PointerTarget
}
