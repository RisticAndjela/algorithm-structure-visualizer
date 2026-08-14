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
