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

Remaining sorting algorithms are still planned and must not be presented as interactive until their Core logic exists.
