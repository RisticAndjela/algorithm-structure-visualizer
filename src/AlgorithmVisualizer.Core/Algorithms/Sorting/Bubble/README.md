# Bubble Sort

`BubbleSortSimulation` provides two manual ascending Bubble Sort variants over a fixed raw `BubbleSortElement[]` teaching array.

## Basic

- adjacent comparisons only: indexes `j` and `j + 1`;
- explicit manual swap using one temporary element reference;
- every canonical shrinking pass runs;
- no early-exit shortcut;
- best / average / worst comparison growth: `Θ(n²)`;
- stable because equal values are never swapped merely for equality;
- extra algorithmic space: `O(1)`.

## Optimized

The optimized variant keeps the same adjacent comparison and swap rule but adds a per-pass `swapped` flag. A complete pass with zero swaps proves the remaining region sorted and stops the run early.

- best case: `Θ(n)`;
- average case: `Θ(n²)`;
- worst case: `Θ(n²)`;
- stable;
- extra algorithmic space: `O(1)`.

## When to use it

Bubble Sort is primarily appropriate for teaching, tiny arrays, or very small nearly-sorted inputs where implementation simplicity matters more than throughput. For larger inputs, prefer an asymptotically stronger algorithm. In the learning path, nearly sorted data should point toward Insertion Sort, while larger stable workloads should point toward Merge Sort.

## Real-time mutation policy

Bubble Sort is **snapshot based**. Arbitrary insert, delete, or update operations during an active run invalidate pair indexes, pass boundaries, and the sorted-suffix invariant. The run must restart after mutation; the UI intentionally locks input editing while playback is active.

The implementation must never delegate to `Array.Sort`, `List.Sort`, LINQ ordering, or another framework/library sorter.
