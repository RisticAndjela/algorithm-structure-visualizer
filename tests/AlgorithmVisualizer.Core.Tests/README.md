# Core Tests

Focused tests for pure Core algorithms and data structures live here. Rendering is intentionally excluded.

## Current coverage

`DataStructures/Trees/Bst/BstSimulationTests.cs` verifies:

- ordered insertion shape;
- strict duplicate rejection;
- found and missing search paths;
- leaf deletion;
- one-child deletion while preserving child object identity;
- two-child deletion using the actual successor node identity;
- height growth for a skewed insertion order.

`DataStructures/Trees/Avl/AvlSimulationTests.cs` verifies:

- all four LL, RR, LR, and RL insertion repairs;
- logarithmic-height behavior for increasing insertion order;
- strict duplicate rejection;
- found and missing search without rotations;
- leaf and one-child deletion, including promoted-child identity;
- delete-triggered upward rebalancing;
- two-child successor transplant while preserving node identity;
- clear/reset behavior;
- BST ordering, parent-link, cached-height, balance-factor, and AVL-balance invariants after mutations.

The tests use a tiny immediate `ISimulationRuntime` fake so algorithm correctness is independent from Blazor playback timing.

`DataStructures/Trees/RedBlack/RedBlackSimulationTests.cs` verifies:

- line-case insertion rotation and root-black restoration;
- red-uncle recoloring;
- strict duplicate rejection;
- logarithmic Red-Black height bound under increasing insertion order;
- search without color mutation;
- two-child delete using the actual successor node identity;
- delete fix-up across a known mutation sequence;
- root-black, BST ordering, parent-link, no-red-red, and equal-black-height invariants after mutations;
- clear/reset behavior.

## Linear Stack / Queue coverage

`DataStructures/Linear/StackQueueSimulationTests.cs` verifies:

- true LIFO stack removal and FIFO queue removal;
- duplicate-value lookup in each structure's real traversal direction;
- stable short displayed-ID lookup;
- delete-by-value compaction in the custom raw array without changing reserved capacity;
- `Count`/`Capacity` behavior after clear.

## Heap coverage

`DataStructures/Heap/DaryHeapSimulationTests.cs` is the unified Heap coverage: Min/Max ordering across multiple branching factors, the `d = 2` Binary Heap case, extract-root repair across all child candidates, arity-change safety, linear value/ID search, generated-starter validity with continued operations, stable surviving element identities after delete, and the shared raw-array behavior.

## Matrix coverage

`DataStructures/Matrix/MatrixSimulationTests.cs` covers row-major storage/resizing, element-wise arithmetic, matrix multiplication, determinant, inverse/singular handling, RREF/rank, solving `A·X=B`, elementary row operations, and the graph-adjacency preset.

- `GraphSimulationTests` covers directed/undirected synchronization, weighted zero-edge presence, duplicate rules, self-loops, vertex deletion, mode guards, identity-preserving rename/weight update, and growth beyond the standalone Matrix page's 8×8 teaching limit.

## Sorting coverage

`Algorithms/Sorting/Bubble/BubbleSortSimulationTests.cs` verifies:

- ascending correctness for the classic example;
- exact comparison/swap/pass counts for the optimized implementation;
- Basic mode still performs all canonical passes on already sorted input;
- one-pass no-swap best-case early exit in Optimized mode;
- reverse-order quadratic worst-case work;
- stable relative identity for duplicate values;
- the one-element zero-comparison boundary.


`Algorithms/Sorting/Selection/SelectionSortSimulationTests.cs` verifies:

- classic ascending output plus exact comparison/swap/pass counts;
- already sorted input still performs `n(n-1)/2` comparisons;
- reverse input keeps the same comparison count while using only direct Selection Sort swaps;
- the `2, 2, 1` counterexample reverses equal-item identity in Classic mode and demonstrates instability;
- Stable Shift sorts the same duplicate case while preserving equal-item identity and avoiding direct swaps;
- a single-element input performs zero comparisons and swaps.


`Algorithms/Sorting/Insertion/InsertionSortSimulationTests.cs` verifies:

- canonical Linear Insertion Sort counts on the classic example;
- adaptive `Θ(n)` sorted-input behavior with zero shifts;
- reverse-input quadratic shifting;
- duplicate stability by original item identity;
- Binary Insertion reducing key comparisons while preserving the same shifts;
- stable upper-bound behavior for duplicates;
- the one-element boundary and online-maintenance capability flags.


`Algorithms/Sorting/Merge/MergeSortSimulationTests.cs` verifies:

- canonical Top-down split/merge counts, exact work, explicit one-item base-case playback, full recursion depth, and preservation of the immutable initial input used by the divide visualization;
- already-sorted Top-down input still follows the canonical recursive split/base-case/merge work;
- Natural Merge recognizes one sorted run and skips merging;
- two existing natural runs collapse in one merge pass;
- stable duplicate identity through left-first equality handling;
- the one-item zero-work boundary and mutation-restart capability flag.


`Algorithms/Sorting/Quick/QuickSortSimulationTests.cs` verifies:

- Basic Lomuto ascending correctness and in-place/no-buffer capability flags;
- the sorted-input last-pivot worst shape (`10` comparisons, `4` partitions, depth `5` for five values);
- Advanced median-of-three + three-way partition reducing recursion depth on sorted input;
- duplicate-heavy Advanced input finalizing an equal-value band without one partition per duplicate;
- the `2, 2, 1` instability counterexample by original item identity;
- the one-element zero-partition boundary and mutation-restart capability flag.

`Algorithms/Sorting/HeapSort/HeapSortSimulationTests.cs` verifies:

- ascending correctness for both Basic Incremental Build and Advanced Floyd Bottom-Up modes;
- the Basic ascending-input build cost (`10` build comparisons / `10` build swaps for seven values);
- Floyd's linear bottom-up build behavior (`8` build comparisons / `4` build swaps for the same input);
- an already valid Max Heap needs zero Floyd build swaps but still requires all root extractions to sort;
- equal-value identity can reverse, demonstrating that Heap Sort is not stable;
- the one-element zero-work boundary and mutation-restart capability flags.


## Search coverage

`Algorithms/Search/LinearSearchSimulationTests.cs` verifies:

- first-index best case with one comparison;
- missing target full scan;
- duplicate target returns the first occurrence;
- empty-array zero-comparison boundary;
- search does not reorder or mutate input values.

`Algorithms/Search/BinarySearchSimulationTests.cs` verifies:
- unsorted Binary Search preprocessing can reuse every implemented sorting algorithm and each produces a valid nondecreasing snapshot before search;
- first-midpoint Θ(1) best case;
- logarithmic missing-target range reduction;
- Basic duplicate search may return a non-first matching midpoint;
- First-occurrence mode continues left and returns the earliest duplicate;
- the Binary Search Core still rejects unsorted input when preprocessing is bypassed, preserving the algorithm precondition;
- the fixed sorted input is never mutated;
- empty-array zero-comparison behavior.

### Graph traversal coverage
- BFS: level-order distances, directed outgoing-only reachability, cycle de-duplication.
- DFS: recursive depth/backtracking, iterative explicit-stack traversal, disconnected components.
- Dijkstra: cheaper detours, Basic/Advanced equivalence, stale priority entries, disconnected infinity, zero-weight support, and negative-weight rejection.

- Topological Sort: Kahn and DFS valid-order checks, multiple sources, cycle detection, weighted-edge ignoring, undirected rejection, and single-vertex boundary.

- MST tests cover Prim/Kruskal minimum-weight agreement, negative/zero weights, disconnected forests, Kruskal cycle rejection, directed precondition rejection, and the single-vertex boundary.


## Vector coverage

`DataStructures/Vector/VectorSimulationTests.cs` verifies raw component order, component-wise addition, orthogonal dot product, unit L2 normalization, zero-vector normalization rejection, parallel cosine similarity, Euclidean/Manhattan distance, and dimension-mismatch rejection.


## Gradient Descent coverage

`MachineLearning/Optimization/GradientDescentSimulationTests.cs` verifies stable fixed-rate convergence, zero-update convergence when theta already equals the target, controlled divergence under an overly aggressive learning rate, Advanced learning-rate decay reducing the effective eta, arbitrary three-parameter optimization, and configuration rejection for dimension mismatch.


## Linear Regression coverage

`MachineLearning/Supervised/LinearRegressionSimulationTests.cs` verifies fitting a simple positive-slope line, zero-update convergence for an already-fitted line, learning a negative trend, the oversized-learning-rate divergence guard, and configuration rejection when X/Y point counts differ.

### KNN coverage

`MachineLearning/Supervised/KnnSimulationTests.cs` checks clear class-0/class-1 neighborhoods, k sensitivity around an outlier, exact-match distance zero, Euclidean-vs-Manhattan nearest-neighbor differences, dimension-independent Core behavior, odd-k validation, and dimension mismatch rejection.
### KD-Tree coverage

`MachineLearning/Supervised/KdTreeSimulationTests.cs` verifies nearest lookup on both sides of the spatial tree, exact-match distance zero, observable pruning, broader backtracking near a split plane, dimension-independent Core behavior with axis cycling beyond 2D, and dimension-mismatch rejection.


## Deep Learning Phase 2 coverage

`MachineLearning/DeepLearning/ActivationMathTests.cs` verifies ReLU vs Leaky ReLU behavior, sigmoid midpoint/derivative, and stable finite sigmoid outputs for very large positive/negative scores.

`MachineLearning/DeepLearning/NeuronSimulationTests.cs` verifies weighted contributions, bias/pre-activation separation, ReLU behavior, sigmoid-at-zero behavior, and shape validation.

`MachineLearning/DeepLearning/MlpSimulationTests.cs` verifies a deterministic one-hidden-layer forward pass and hidden ReLU clamping.

`MachineLearning/DeepLearning/BackpropagationSimulationTests.cs` verifies finite output/hidden gradients, a full reverse chain, and that one learning update reduces or preserves the teaching loss for the tested cases.

`MachineLearning/DeepLearning/OptimizerSimulationTests.cs` runs SGD, Momentum, and Adam over the same deterministic sample stream, checks finite parameter paths and MSE improvement, and validates dataset-shape rejection.


## Graph ML Phase 3 coverage

`MachineLearning/GraphMl/SparseMatrixSimulationTests.cs` verifies exact CSR value/column/row-pointer layout, sparse matrix-vector multiplication, direct `ManualCsrMatrix` access, and a case where sparse storage uses fewer slots than the dense grid.

`MachineLearning/GraphMl/PageRankSimulationTests.cs` verifies uniform rank on a directed cycle, a clear incoming hub becoming top-ranked, dangling-node probability redistribution, finite positive ranks, and rank-mass preservation.

`MachineLearning/GraphMl/SpectralClusteringSimulationTests.cs` verifies the project-owned Jacobi eigensolver on a known diagonal matrix, connectivity-based separation of two graph communities, sorted finite spectral state, and rejection of isolated nodes for the normalized teaching Laplacian.

`MachineLearning/GraphMl/MessagePassingSimulationTests.cs` verifies synchronous one-layer mean aggregation with exact expected vectors, isolated-node zero-neighbor behavior, and two-hop propagation after two sum-aggregation layers.
