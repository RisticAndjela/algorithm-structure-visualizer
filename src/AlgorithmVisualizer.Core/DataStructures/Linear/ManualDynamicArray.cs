using System.Collections;

namespace AlgorithmVisualizer.Core.DataStructures.Linear;

/// <summary>
/// Small teaching-oriented dynamic array implemented from scratch for the live linear structures.
/// It deliberately does not use List&lt;T&gt;, Stack&lt;T&gt;, Queue&lt;T&gt;, Array.Copy, or other
/// collection helpers for storage mutation. Capacity growth and deletion shifts are explicit loops.
/// </summary>
public sealed class ManualDynamicArray<T> : IReadOnlyList<T>
{
    private const int FirstCapacity = 4;

    private T[] _buffer = new T[0];

    public int Count { get; private set; }
    public int Capacity => _buffer.Length;

    public T this[int index]
    {
        get
        {
            ValidateExistingIndex(index);
            return _buffer[index];
        }
    }

    public void Add(T item)
    {
        EnsureCapacityForOneMoreItem();
        _buffer[Count] = item;
        Count++;
    }

    public void RemoveAt(int index)
    {
        ValidateExistingIndex(index);

        // Close the logical gap manually. No List.RemoveAt / Array.Copy is used.
        for (var current = index; current < Count - 1; current++)
        {
            _buffer[current] = _buffer[current + 1];
        }

        Count--;
        _buffer[Count] = default!;
    }

    public void Clear()
    {
        // Release references held by used slots while deliberately keeping capacity reserved.
        for (var index = 0; index < Count; index++)
        {
            _buffer[index] = default!;
        }

        Count = 0;
    }

    private void EnsureCapacityForOneMoreItem()
    {
        if (Count < Capacity)
        {
            return;
        }

        var newCapacity = Capacity == 0 ? FirstCapacity : Capacity * 2;
        var expanded = new T[newCapacity];

        // Copy existing items manually so the resizing algorithm remains visible in our own code.
        for (var index = 0; index < Count; index++)
        {
            expanded[index] = _buffer[index];
        }

        _buffer = expanded;
    }

    private void ValidateExistingIndex(int index)
    {
        if (index < 0 || index >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var index = 0; index < Count; index++)
        {
            yield return _buffer[index];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
