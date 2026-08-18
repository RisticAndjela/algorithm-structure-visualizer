using AlgorithmVisualizer.Core.DataStructures.Heap;

namespace AlgorithmVisualizer.Core.Algorithms.GraphSpanningTree.Mst;

/// <summary>
/// Prim-specific min-edge frontier backed by the project's existing ManualHeapArray.
/// No framework PriorityQueue or sorted collection is used.
/// </summary>
internal sealed class ManualMinEdgeFrontier
{
    private readonly ManualHeapArray<Entry> _items = new();

    public int Count => _items.Count;
    public int ComparisonCount { get; private set; }

    public void Clear()
    {
        _items.Clear();
        ComparisonCount = 0;
    }

    public void Push(int edgeIndex, int fromIndex, int toIndex, double weight)
    {
        _items.Add(new Entry(edgeIndex, fromIndex, toIndex, weight));
        var index = _items.Count - 1;
        while (index > 0)
        {
            var parent = (index - 1) / 2;
            ComparisonCount++;
            if (!Less(_items[index], _items[parent])) break;
            _items.Swap(index, parent);
            index = parent;
        }
    }

    public Entry PopMin()
    {
        if (_items.Count == 0) throw new InvalidOperationException("The Prim frontier is empty.");
        var minimum = _items[0];
        if (_items.Count == 1)
        {
            _items.RemoveLast();
            return minimum;
        }

        _items[0] = _items[_items.Count - 1];
        _items.RemoveLast();
        var index = 0;
        while (true)
        {
            var left = index * 2 + 1;
            if (left >= _items.Count) break;
            var right = left + 1;
            var best = left;
            if (right < _items.Count)
            {
                ComparisonCount++;
                if (Less(_items[right], _items[left])) best = right;
            }
            ComparisonCount++;
            if (!Less(_items[best], _items[index])) break;
            _items.Swap(index, best);
            index = best;
        }
        return minimum;
    }

    public MstFrontierEntrySnapshot[] Snapshot(bool[] inForest)
    {
        var result = new MstFrontierEntrySnapshot[_items.Count];
        for (var index = 0; index < _items.Count; index++)
        {
            var item = _items[index];
            var stale = item.ToIndex < 0 || item.ToIndex >= inForest.Length || inForest[item.ToIndex];
            result[index] = new MstFrontierEntrySnapshot(item.EdgeIndex, item.FromIndex, item.ToIndex, item.Weight, stale);
        }
        return result;
    }

    private static bool Less(Entry left, Entry right)
    {
        if (left.Weight < right.Weight) return true;
        if (left.Weight > right.Weight) return false;
        return left.EdgeIndex < right.EdgeIndex;
    }

    internal readonly record struct Entry(int EdgeIndex, int FromIndex, int ToIndex, double Weight);
}
