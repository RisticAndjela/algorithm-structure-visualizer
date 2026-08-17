# Insertion Sort

`InsertionSortSimulation` implements stable ascending Insertion Sort manually over a fixed teaching array. No framework sorter or ordered collection performs the algorithm.

## Variants

- **Linear** — canonical Insertion Sort. Hold `a[i]`, scan the sorted prefix from right to left, shift every value strictly greater than the key, then insert the key into the gap. Best case is `Θ(n)` on already/nearly-sorted input; average/worst are `Θ(n²)`.
- **BinarySearch** — Binary Insertion Sort. Use a stable upper-bound binary search over the sorted prefix to locate the insertion point, then perform the same explicit shifts. It reduces key comparisons to roughly `Θ(n log n)` across all insertion-point searches, but array movement can still be `Θ(n²)`, so worst-case total time stays quadratic.

Both variants are stable and use `O(1)` extra algorithmic space. The teaching model exposes the held key and temporary gap explicitly so the memory effect of shifting is visible.

## Online / CRUD behavior

Insertion Sort is naturally online. Once a collection is sorted:

- READ/inspection is safe;
- CREATE/append can be treated as a new key and inserted into the sorted collection without a full re-sort;
- DELETE preserves sorted order (array-backed storage may still need compaction);
- UPDATE can be repaired by removing the changed item and reinserting it at its new ordered position.

The Blazor playback history is still snapshot based. Mutating data during an active recorded trace starts a new trace rather than pretending old frames remain valid.

## Practical fit

Use Linear Insertion Sort for small arrays, nearly sorted data, incremental arrivals, or tiny subarrays inside a hybrid strategy. Binary Insertion is useful when comparisons are expensive enough that reducing them matters. For large general-purpose stable sorting, Merge Sort is the better next algorithm because it avoids quadratic movement at the cost of extra memory.
