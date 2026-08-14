namespace AlgorithmVisualizer.Core.DataStructures.Heap;

/// <summary>
/// Minimal growable array implemented specifically for the heap teaching module.
/// No List&lt;T&gt;, Array.Copy, collection heap, or sorting helper owns heap storage or mutation.
/// </summary>
internal sealed class ManualHeapArray<T>
{
    private const int FirstCapacity = 4;
    private T[] _buffer = Array.Empty<T>();

    public int Count { get; private set; }
    public int Capacity => _buffer.Length;

    public T this[int index]
    {
        get
        {
            ValidateIndex(index);
            return _buffer[index];
        }
        set
        {
            ValidateIndex(index);
            _buffer[index] = value;
        }
    }

    public void Add(T item)
    {
        EnsureCapacityForOneMore();
        _buffer[Count] = item;
        Count++;
    }

    public T RemoveLast()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Cannot remove from an empty heap array.");
        }

        var lastIndex = Count - 1;
        var removed = _buffer[lastIndex];
        _buffer[lastIndex] = default!;
        Count--;
        return removed;
    }

    public void Swap(int firstIndex, int secondIndex)
    {
        ValidateIndex(firstIndex);
        ValidateIndex(secondIndex);

        if (firstIndex == secondIndex)
        {
            return;
        }

        var temporary = _buffer[firstIndex];
        _buffer[firstIndex] = _buffer[secondIndex];
        _buffer[secondIndex] = temporary;
    }

    public void Clear()
    {
        for (var index = 0; index < Count; index++)
        {
            _buffer[index] = default!;
        }

        Count = 0;
    }

    private void EnsureCapacityForOneMore()
    {
        if (Count < Capacity)
        {
            return;
        }

        var newCapacity = Capacity == 0 ? FirstCapacity : Capacity * 2;
        var expanded = new T[newCapacity];

        for (var index = 0; index < Count; index++)
        {
            expanded[index] = _buffer[index];
        }

        _buffer = expanded;
    }

    private void ValidateIndex(int index)
    {
        if (index < 0 || index >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}
