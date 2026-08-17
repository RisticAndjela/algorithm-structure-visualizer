# Algorithms

Algorithm implementations live in this pure C# Core layer and do not depend on Blazor, CSS, DOM APIs, or browser state.

## Implemented sorting algorithms

### Bubble Sort

Route: `/sorting/bubble`

Core namespace: `AlgorithmVisualizer.Core.Algorithms.Sorting.Bubble`

The implementation is written from scratch over a primitive raw array of teaching elements. It does not call `Array.Sort`, `List.Sort`, LINQ ordering, or another sorting implementation.

The live algorithm exposes semantic playback steps for:

- the start of a run;
- each adjacent comparison `a[j]` vs `a[j+1]`;
- the swap/keep decision;
- every explicit neighboring swap;
- completion of a pass and growth of the sorted suffix;
- the optimized no-swap early exit;
- final completion.

The implementation is ascending and stable: it swaps only when the left value is strictly greater than the right value, so equal-valued element identities retain their relative order.

Complexity taught by the module:

- best case with no-swap early exit: `Θ(n)`;
- average case: `Θ(n²)`;
- worst case: `Θ(n²)`;
- algorithmic extra space: `O(1)`.

### Selection Sort

Route: `/sorting/selection`

Core namespace: `AlgorithmVisualizer.Core.Algorithms.Sorting.Selection`

The classic ascending implementation is written from scratch over a fixed raw array of teaching elements. Each pass scans the full unsorted suffix, remembers the smallest candidate index, and performs at most one direct swap into the next target position. For `n >= 2`, it therefore performs exactly `n(n-1)/2` comparisons regardless of input order, uses at most `n-1` swaps when self-swaps are skipped, and uses `O(1)` extra algorithmic space. The direct-swap variant is intentionally taught as not stable; element identity is preserved so the `2, 2, 1` counterexample is observable.

### Insertion Sort

Route: `/sorting/insertion`

Core namespace: `AlgorithmVisualizer.Core.Algorithms.Sorting.Insertion`

The implementation is written from scratch over a fixed nullable raw teaching array so the held key and temporary gap are explicit. **Linear** mode performs canonical stable backward scanning and right shifts. **BinarySearch** mode uses a stable upper-bound binary search to locate the insertion point before performing the same explicit shifts. No framework sorting/searching collection performs the taught behavior.

Complexity taught by the module:

- Linear best case on already-sorted input: `Θ(n)`;
- Binary Insertion search work across passes: about `Θ(n log n)` comparisons, but total worst-case time remains `Θ(n²)` because shifts still dominate;
- average/worst general movement: `Θ(n²)`;
- algorithmic extra space: `O(1)`;
- both variants are stable.

Insertion Sort is also taught as an online algorithm: after the collection is sorted, a new item can be inserted incrementally, delete preserves order, and update can be repaired by remove + reinsert. UI playback frames are still snapshot-specific and restart after mutation.

### Merge Sort

Route: `/sorting/merge`

Core namespace: `AlgorithmVisualizer.Core.Algorithms.Sorting.Merge`

The implementation is written from scratch over stable teaching elements plus one reusable auxiliary array. **TopDownRecursive** performs canonical midpoint splitting and recursive merge-up. **NaturalRuns** scans for maximal nondecreasing runs and merges neighboring runs directly, allowing a fully sorted input to finish after one `Θ(n)` run-detection scan with zero merges. Both variants merge stably by choosing the left-run item first when values are equal.

Complexity taught by the module:

- Top-down best/average/worst: `Θ(n log n)`;
- Natural Merge best case on one existing run: `Θ(n)`;
- Natural Merge worst case: `Θ(n log n)`;
- reusable auxiliary buffer: `O(n)`;
- top-down recursion stack: `O(log n)`.

Active create/update/delete mutations require restart because range boundaries, run boundaries, and buffer contents belong to the old snapshot.

Quick Sort and Heap Sort are still planned and must not be presented as interactive until their Core logic exists.
