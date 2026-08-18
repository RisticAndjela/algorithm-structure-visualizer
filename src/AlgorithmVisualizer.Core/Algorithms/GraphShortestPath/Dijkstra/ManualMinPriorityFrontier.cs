using AlgorithmVisualizer.Core.DataStructures.Heap;

namespace AlgorithmVisualizer.Core.Algorithms.GraphShortestPath.Dijkstra;

/// <summary>
/// Dijkstra-specific priority frontier built on the project's existing ManualHeapArray storage.
/// It deliberately does not use PriorityQueue&lt;TElement,TPriority&gt; or another framework heap.
/// Distance decreases are represented by pushing a new entry; stale entries are skipped on pop.
/// </summary>
internal sealed class ManualMinPriorityFrontier
{
    private readonly ManualHeapArray<Entry> _items = new();

    public int Count => _items.Count;
    public int ComparisonCount { get; private set; }

    public void Clear()
    {
        _items.Clear();
        ComparisonCount = 0;
    }

    public void Push(int vertexIndex, double priority)
    {
        _items.Add(new Entry(vertexIndex, priority));
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
        if (_items.Count == 0) throw new InvalidOperationException("The priority frontier is empty.");

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

    public DijkstraFrontierEntrySnapshot[] Snapshot(double[] distances, bool[] settled)
    {
        var result = new DijkstraFrontierEntrySnapshot[_items.Count];
        for (var index = 0; index < _items.Count; index++)
        {
            var entry = _items[index];
            var stale = entry.VertexIndex < 0 || entry.VertexIndex >= distances.Length ||
                        settled[entry.VertexIndex] ||
                        !SameDistance(entry.Priority, distances[entry.VertexIndex]);
            result[index] = new DijkstraFrontierEntrySnapshot(entry.VertexIndex, entry.Priority, stale);
        }
        return result;
    }

    private static bool Less(Entry left, Entry right)
    {
        if (left.Priority < right.Priority) return true;
        if (left.Priority > right.Priority) return false;
        return left.VertexIndex < right.VertexIndex;
    }

    private static bool SameDistance(double left, double right) => Math.Abs(left - right) <= 1e-9;

    internal readonly record struct Entry(int VertexIndex, double Priority);
}
