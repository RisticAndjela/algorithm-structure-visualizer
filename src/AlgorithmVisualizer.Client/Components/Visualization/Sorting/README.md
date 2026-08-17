# Sorting visualization

All live sorting labs follow the same learner contract: **Build → Predict → Watch → Explain → Practice**, plus an explicit implementation-choice and workload-fit layer.

Every sorting page must explain:

- the canonical/basic implementation;
- an advanced/optimized implementation when a meaningful one exists;
- when the algorithm is useful and when it should be avoided;
- whether real-time insert/delete/update changes can continue safely or require restart;
- which other sorting algorithm is a better fit for common alternative workloads.

## Bubble Sort — live

Bubble Sort exposes two real Core modes:

- **Basic** — canonical shrinking passes with no early exit (`Θ(n²)` even on sorted input);
- **Optimized** — the same stable neighbor rule plus a no-swap early exit (`Θ(n)` best case).

The page teaches the sorted suffix, adjacent-only comparisons, duplicate stability, fixed-array Memory state, real run metrics, and the rule that arbitrary CRUD mutation invalidates the current trace and requires restart. Workload guidance points nearly sorted data toward Insertion Sort and larger stable data toward Merge Sort.

## Selection Sort — live

Selection Sort exposes two real Core modes:

- **Classic** — direct minimum swap, at most `n-1` swaps, not stable;
- **Stable Shift** — hold the minimum, shift the intermediate block right, then insert; stable but uses more array writes.

Both preserve the canonical full minimum scan and therefore remain `Θ(n²)`. The page teaches destination/minimum/scan roles, sorted-prefix invariants, mutation restart semantics, and workload guidance toward Heap Sort or Merge Sort when appropriate.

## Insertion Sort — live

Insertion Sort exposes two real Core modes:

- **Linear** — canonical stable backward scan; adaptive with a `Θ(n)` best case on already-sorted input;
- **Binary Insertion** — stable upper-bound binary search for the insertion point; fewer key comparisons, but the same explicit array shifts and `Θ(n²)` worst-case total time.

The page makes the held key and temporary gap explicit in both Visual and Memory views. It teaches the sorted-prefix invariant, adaptive behavior, duplicate stability, practical workload fit, and the online CRUD property: a sorted collection can accept incremental insertions without a full re-sort, delete preserves order, and update can be repaired by remove + reinsert. Recorded playback frames remain snapshot based and restart after mutation.

Merge, Quick, and Heap Sort remain TODO placeholders until their Core implementations are added.
