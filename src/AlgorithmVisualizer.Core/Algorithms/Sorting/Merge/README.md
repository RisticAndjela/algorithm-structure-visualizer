# Merge Sort

The Merge Sort module is implemented manually in `MergeSortSimulation`; it does not call framework sorting helpers.

## Variants

- **Basic — Top-down Recursive Merge Sort**: recursively splits a range in half until one-item runs remain, then merges upward. Best, average, and worst time are `Θ(n log n)`.
- **Advanced — Natural Merge Sort**: scans for maximal nondecreasing runs already present in the data and merges neighboring runs. A fully sorted input is recognized in `Θ(n)`; worst-case time remains `Θ(n log n)`.

Both variants are stable. During merging, equal values are taken from the left run first (`left <= right`), preserving duplicate identity order. One reusable auxiliary buffer of length `n` gives `O(n)` extra array storage; top-down recursion also uses `O(log n)` call-stack depth.

## Mutation policy

Reads do not invalidate a run. `CREATE`, `UPDATE`, or `DELETE` during an active Merge Sort changes run boundaries and buffer assumptions, so the active run must restart from the new data snapshot. Ordered maintenance after a completed sort is a separate operation, not Merge Sort continuing online.

## Teaching state

Snapshots expose the active range, left/right run boundaries, read fronts, next buffer/copy-back slot, comparisons, writes, merge count, recursion depth, natural-run count, and the reusable auxiliary buffer. The client uses the same snapshots for Visual State, Memory State, timeline review, explanations, and verified practice tasks.
