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

## Heap coverage

`DataStructures/Heap/HeapSimulationTests.cs` verifies both Min Heap and Max Heap ordering, bubble-up, extract-root bubble-down, arbitrary search, delete repair, duplicate element identities, heap-kind switching rules, and manual backing-array capacity behavior.


`DataStructures/Heap/DaryHeapSimulationTests.cs` verifies generalized d-ary Min/Max ordering for multiple branching factors, extract-root repair across all child candidates, the mathematical `d = 2` equivalence with Binary Heap indexing, arity-change safety, O(n) missing search, and stable surviving element identities after delete.

## Matrix coverage

`DataStructures/Matrix/MatrixSimulationTests.cs` covers row-major storage/resizing, element-wise arithmetic, matrix multiplication, determinant, inverse/singular handling, RREF/rank, solving `A·X=B`, elementary row operations, and the graph-adjacency preset.

- `GraphSimulationTests` covers directed/undirected synchronization, weighted zero-edge presence, duplicate rules, self-loops, vertex deletion, mode guards, identity-preserving rename/weight update, and growth beyond the standalone Matrix page's 8×8 teaching limit.
