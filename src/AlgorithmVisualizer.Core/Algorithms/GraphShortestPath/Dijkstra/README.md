# Dijkstra

The module runs on the existing `GraphSnapshot` and rejects negative edge weights.

- **Basic / LinearScan** chooses the next unsettled minimum with an explicit vertex scan: `O(V² + E)`.
- **Advanced / MinHeap** uses a Dijkstra-specific binary min-heap built on the existing `ManualHeapArray` storage frontier with lazy duplicate entries: `O((V + E) log V)`.
- Both variants perform the same relaxation rule and produce the same shortest-path distances for non-negative weights.
- No `PriorityQueue<TElement,TPriority>`, graph package, or shortest-path library is used.
- Zero-weight edges are supported; unreachable distances remain infinity.
