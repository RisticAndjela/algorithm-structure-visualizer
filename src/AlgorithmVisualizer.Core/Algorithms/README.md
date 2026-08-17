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

Insertion Sort, Merge Sort, Quick Sort, and Heap Sort are still planned and must not be presented as interactive until their Core logic exists.
