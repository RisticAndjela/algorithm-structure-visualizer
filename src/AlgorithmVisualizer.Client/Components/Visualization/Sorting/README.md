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

## Merge Sort — live

Merge Sort exposes two real Core modes:

- **Top-down Recursive** — canonical divide-and-conquer midpoint splitting and recursive merge-up; `Θ(n log n)` best/average/worst;
- **Natural Merge** — detects maximal nondecreasing runs already present in the input; adaptive `Θ(n)` best case for one sorted run and `Θ(n log n)` worst case.

Basic Top-down Visual state uses a single-focus teaching view: during divide steps it shows only the active parent range and the two child ranges it creates; during merge steps that divide view disappears and the learner sees only the two already-sorted child runs, the temporary buffer, and copy-back. The main array stays visible as the stable reference at the bottom. Natural Merge intentionally uses a separate detected-runs presentation instead of pretending it follows recursive midpoint splitting.

Both modes use one reusable `O(n)` auxiliary buffer and preserve duplicate identity by selecting from the left run on equality. Visual State exposes active ranges, run fronts, buffer writes, and copy-back. Memory State shows the main array and auxiliary buffer side by side. Reads are safe during analysis, but active create/update/delete mutations require a new run because range/run boundaries and playback frames belong to the previous snapshot.


## Quick Sort — live

Quick Sort exposes two real Core modes:

- **Basic Lomuto** — last element pivot, one `<= pivot` boundary, then explicit pivot placement;
- **Advanced median-of-three + 3-way** — median heuristic for first/middle/last candidates plus `<`, `=`, and `>` regions so duplicate-heavy inputs do not recurse through the full equal band.

Visual State keeps one active partition in focus and labels pivot, scan, classified regions, and finalized indexes. Memory State shows the fixed array slots, stable item identity labels (to demonstrate that Quick Sort itself is not stable), and the recursion-stack cost: `O(log n)` average, `O(n)` worst. Reads are safe, but create/update/delete during partitioning requires a new run because the old pivot/range trace is no longer valid.

Quick Sort is live with Basic Lomuto and Advanced median-of-three + 3-way partition visualizations.

## Heap Sort — live

Heap Sort exposes two real Core construction modes:

- **Basic Incremental Build** — grows a Max Heap prefix with insertion + bubble-up (`O(n log n)` build);
- **Advanced Floyd Bottom-Up** — heapifies parents from the last parent to the root (`Θ(n)` build).

Both then use the same in-place root extraction and sift-down loop. Visual State deliberately shows the same objects as a complete binary tree and as one array separated into `ACTIVE HEAP | SORTED SUFFIX`. Memory State removes the teaching tree and exposes the actual fixed array slots, item identity, and `O(1)` extra-array cost. Heap Sort is not stable and create/update/delete mutations require a new run because they invalidate both heap order and suffix-finality.
