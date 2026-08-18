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

### Quick Sort

Route: `/sorting/quick`

Core namespace: `AlgorithmVisualizer.Core.Algorithms.Sorting.Quick`

The implementation is manual and in-place over `QuickSortElement[]`. **LomutoLastPivot** uses the last element as pivot and one growing `<= pivot` boundary. **MedianOfThreeThreeWay** chooses the median of first/middle/last values, then builds `< pivot`, `= pivot`, and `> pivot` regions so duplicate-heavy inputs can finish the equal band without recurring through it.

Complexity taught by the module:

- best/average: `Θ(n log n)` when partitions stay reasonably balanced;
- worst: `Θ(n²)` when pivots repeatedly create highly unbalanced ranges;
- extra array storage: `O(1)`;
- recursion stack: `O(log n)` average and `O(n)` worst;
- both variants are intentionally not stable.

Active create/update/delete mutations require restart because pivot choices, indexes, and recursive partition ranges belong to the old snapshot.

### Heap Sort

Route: `/sorting/heap`

Core namespace: `AlgorithmVisualizer.Core.Algorithms.Sorting.HeapSort`

The implementation is written from scratch over one `HeapSortElement[]`. **IncrementalBuild** grows a Max Heap prefix with explicit parent comparisons and bubble-up. **FloydBottomUp** heapifies from the last parent toward the root with explicit sift-down, giving a `Θ(n)` build phase. Both variants then swap the maximum root into the final suffix, shrink `heapSize`, and sift the new root down inside the reduced heap.

Heap Sort keeps `Θ(n log n)` best/average/worst total time and `O(1)` extra array storage. It is not stable. Active create/update/delete mutations require restart because heap order, the heap boundary, and sorted-suffix finality belong to the old snapshot.

Bubble Sort, Selection Sort, Insertion Sort, Merge Sort, Quick Sort, and Heap Sort are all live.


## Implemented search algorithms

### Linear Search

Route: `/search/linear`

Core namespace: `AlgorithmVisualizer.Core.Algorithms.Search.Linear`

The implementation manually scans a fixed raw array from index `0` to `n-1`, compares one value per visited slot, stops on the first match, and performs no element mutation. It does not call LINQ `First`/`Contains`, `Array.IndexOf`, or another search helper.

Complexity taught by the module:

- first index match: `Θ(1)`;
- average case: `Θ(n)`;
- last-index first occurrence or missing target: `Θ(n)`;
- algorithmic extra space: `O(1)`.

The UI makes duplicate first-occurrence behavior and zero-write Memory State explicit.

### Binary Search

Route: `/search/binary`

Core namespace: `AlgorithmVisualizer.Core.Algorithms.Search.Binary`

Binary Search operates only on a nondecreasing input snapshot and manually maintains `left`, `right`, and `mid` bounds. Basic mode returns any midpoint match. Advanced first-occurrence mode preserves the same logarithmic search strategy but continues into the left half after equality to locate the first duplicate occurrence. The client can preprocess an unsorted input by silently running any already implemented sorting Core algorithm before the Binary Search run.

Complexity taught by the module:

- best case: `Θ(1)`;
- average/worst search: `Θ(log n)`;
- algorithmic extra space: `O(1)`.

### Breadth-First Search (BFS)

Route: `/search/bfs`

Core namespace: `AlgorithmVisualizer.Core.Algorithms.GraphTraversal`

BFS reuses the existing graph snapshot and manually implements its FIFO frontier with `ManualDynamicArray<int>` plus a head cursor rather than framework `Queue<T>`. A vertex is marked visited when it is enqueued, each reachable vertex is discovered at most once, and parent plus unweighted distance are recorded. Directed graphs follow outgoing adjacency only; edge weights are ignored because this is unweighted traversal.

Complexity taught by the module:

- traversal time: `O(V + E)`;
- traversal state/frontier: `O(V)`.

### Depth-First Search (DFS)

Route: `/search/dfs`

Core namespace: `AlgorithmVisualizer.Core.Algorithms.GraphTraversal`

DFS has two live variants. Recursive mode uses real recursive calls and exposes the call stack plus backtracking. Iterative mode uses `ManualDynamicArray<int>` as an explicit LIFO frontier rather than framework `Stack<T>`. Both variants share visited/parent/depth state, terminate safely on cycles and self-loops, and follow outgoing adjacency in directed graphs.

Complexity taught by the module:

- traversal time: `O(V + E)`;
- recursion or explicit stack plus visited state: `O(V)`.

### Dijkstra shortest paths

Route: `/graph-algorithms/dijkstra`

Core namespace: `AlgorithmVisualizer.Core.Algorithms.GraphShortestPath.Dijkstra`

Dijkstra consumes the existing `GraphSnapshot`, rejects negative weights, permits zero weights, and records `dist[]`, `parent[]`, `settled[]`, settlement order, edge checks and relaxation updates. Basic mode selects the next minimum with an explicit linear scan (`O(V² + E)`). Advanced mode uses a project-owned binary min-heap with lazy priority entries (`O((V + E) log V)`) and never delegates to `PriorityQueue<TElement,TPriority>`.

Linear Search, Binary Search, BFS, DFS, Dijkstra, and Topological Sort are live.


### Topological Sort

Core namespace: `AlgorithmVisualizer.Core.Algorithms.GraphOrdering.Topological`

Topological Sort consumes the existing directed `GraphSnapshot`. Kahn mode uses explicit `indegree[]` plus a head-index FIFO over `ManualDynamicArray<int>`. DFS mode uses recursive white/gray/black visitation plus a manual postorder buffer and manual reverse. Both are `O(V + E)` time / `O(V)` extra space, ignore edge weights, reject undirected input, and report directed cycles rather than returning a misleading ordering.
