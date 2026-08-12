namespace AlgorithmVisualizer.Core.DataStructures.Linear;

/// <summary>
/// Represents one value in a linear data structure together with renderer-neutral
/// transient state used by visual simulations.
/// </summary>
public sealed class LinearElement
{
    public LinearElement(int value, LinearElementVisualState visualState = LinearElementVisualState.Normal)
        : this(Guid.NewGuid(), value, visualState)
    {
    }

    /// <summary>
    /// Recreates the same logical element for read-only timeline playback.
    /// The ID is preserved so the learning UI can step backward without inventing a new identity.
    /// </summary>
    public LinearElement(Guid id, int value, LinearElementVisualState visualState = LinearElementVisualState.Normal)
    {
        Id = id;
        Value = value;
        VisualState = visualState;
    }

    public LinearElement Snapshot() => new(Id, Value, VisualState);

    public Guid Id { get; }
    public string DisplayId => Id.ToString("N")[..6].ToUpperInvariant();
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
    PointerTarget,
    Checking,
    Visited,
    Matched
}
