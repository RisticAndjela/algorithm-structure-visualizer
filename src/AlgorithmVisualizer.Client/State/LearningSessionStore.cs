namespace AlgorithmVisualizer.Client.State;

/// <summary>
/// Keeps lightweight learning preferences and practice progress in the Blazor
/// WebAssembly process without relying on JavaScript browser-storage interop.
///
/// The store intentionally has session lifetime: values survive navigation while
/// the application is running, but a full browser reload starts a new WebAssembly
/// process and therefore clears this state. Durable persistence can later be
/// provided by a C# HTTP API/database without reintroducing JavaScript.
/// </summary>
public sealed class LearningSessionStore
{
    private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);

    public string? GetItem(string key)
    {
        ValidateKey(key);
        return _values.TryGetValue(key, out var value) ? value : null;
    }

    public void SetItem(string key, string value)
    {
        ValidateKey(key);
        ArgumentNullException.ThrowIfNull(value);
        _values[key] = value;
    }

    public void RemoveItem(string key)
    {
        ValidateKey(key);
        _values.Remove(key);
    }

    public void Clear() => _values.Clear();

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("A learning-state key is required.", nameof(key));
        }
    }
}
