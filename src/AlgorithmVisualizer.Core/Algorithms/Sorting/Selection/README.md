# Selection Sort

`SelectionSortSimulation` provides two manual ascending Selection Sort placement strategies over a fixed raw `SelectionSortElement[]` teaching array. Both variants perform the same full minimum scan for every target index.

## Classic

- sorted prefix grows from left to right;
- each pass scans the entire unsorted suffix for the minimum;
- at most one direct swap places the minimum at the target;
- comparisons are always `n(n-1)/2` for `n >= 2`;
- direct swaps are at most `n-1` when self-swaps are skipped;
- extra algorithmic space: `O(1)`;
- **not stable** because a long-distance swap can move equal items across each other.

Classic Selection Sort is useful mainly for tiny arrays and situations where minimizing expensive writes/swaps matters more than minimizing comparisons.

## Stable Shift

Stable Shift keeps the same minimum scan but changes placement:

1. hold the selected minimum in one temporary element reference;
2. shift the block from `target..min-1` one slot right;
3. insert the held minimum at `target`.

This preserves the relative order of equal values and remains `O(1)` in extra algorithmic space, but it can perform substantially more array-slot writes than Classic mode. It does **not** improve the `Θ(n²)` comparison complexity.

For larger inputs, Heap Sort is the natural conceptual upgrade to repeated selection with `O(n log n)` time. When stability and scale both matter, prefer Merge Sort.

## Real-time mutation policy

Selection Sort is **snapshot based**. Arbitrary insert, delete, or update operations during a run can invalidate the minimum selected for a pass or even the already-fixed prefix. A mutated collection therefore requires a new run.

The implementation must never delegate to `Array.Sort`, `List.Sort`, LINQ ordering, or another sorting helper.
